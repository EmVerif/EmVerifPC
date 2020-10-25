using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class CanDiagSendCommand : IEmVerifCommand
    {
        private enum State
        {
            WaitStart,
            WaitEnd
        }

        private class DataMask
        {
            public Int32 StartIdx;
            public List<Byte> MaskList = new List<byte>();
            public Int32 LShift;
            public string RefVar;

            public DataMask(PublicApis.CanDataMask dataMask)
            {
                StartIdx = ((Int32)dataMask.BytePos * 8 + (7 - (Int32)dataMask.BitPos) - ((Int32)dataMask.BitLen - 1)) / 8;
                if ((StartIdx < 0) || (dataMask.BitLen > 64) || (dataMask.BitPos >= 8))
                {
                    throw new Exception("DataMask設定エラー。");
                }
                int maskListCount = (int)Math.Ceiling((float)(dataMask.BitPos + dataMask.BitLen) / 8);
                LShift = (Int32)(dataMask.BitPos);
                Byte lastMsk = (Byte)((0xFF << LShift) & 0xFF);
                Byte fstMsk = (Byte)(0xFF >> ((128 - (int)dataMask.BitPos - (int)dataMask.BitLen) % 8));

                for (int idx = 0; idx < maskListCount; idx++)
                {
                    Byte msk = 0xFF;

                    if (idx == 0)
                    {
                        msk &= fstMsk;
                    }
                    if (idx == (maskListCount - 1))
                    {
                        msk &= lastMsk;
                    }
                    MaskList.Add(msk);
                }
                RefVar = dataMask.RefVar;
            }
        }

        public const UInt32 NoValue = 0xFFFFFFFF;

        private string _NextState;
        private string _StopState;

        private IReadOnlyList<Byte> _SendDataList;
        private List<DataMask> _DataMaskList;
        private UInt32 _SendCanId;
        private UInt32 _SendNta;
        private UInt32 _ResponseCanId;
        private UInt32 _ResponseNta;
        private string _ResponseDataArrayName;
        private UInt32 _RepeatTimeMs;

        private List<Byte> _ResponseDataList = new List<byte>();
        private Boolean _TimeoutFlag;
        private Boolean _ErrorSeqFlag;

        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_][\w\[\]]*)");
        private State _State;
        private UInt32 _PrevTimestampMs;
        private UInt32 _TimingMs;
        private DataTable _Dt = new DataTable();

        public CanDiagSendCommand(
            string inNext,
            UInt32 inSendCanId,
            IReadOnlyList<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            IReadOnlyList<PublicApis.CanDataMask> inDataMaskList,
            UInt32 inSendNta,
            UInt32 inResponseCanId,
            UInt32 inResponseNta,
            string inResponseDataArrayName
        )
        {
            _NextState = inNext;
            _StopState = null;

            if (inSendDataList.Count > 4095)
            {
                throw new Exception("CanDiagSendの送信最大サイズは4095[B]です。");
            }
            _SendDataList = inSendDataList;
            _DataMaskList = new List<DataMask>();
            if (inDataMask != null)
            {
                _DataMaskList.Add(new DataMask(inDataMask));
            }
            if (inDataMaskList != null)
            {
                foreach (var dataMask in inDataMaskList)
                {
                    _DataMaskList.Add(new DataMask(dataMask));
                }
            }
            foreach (var msk in _DataMaskList)
            {
                if ((msk.StartIdx + msk.MaskList.Count) > _SendDataList.Count)
                {
                    throw new Exception("DataMask設定エラー。");
                }
            }
            _SendCanId = inSendCanId;
            _SendNta = inSendNta;
            _ResponseCanId = inResponseCanId;
            _ResponseNta = inResponseNta;
            _ResponseDataArrayName = inResponseDataArrayName;
            _RepeatTimeMs = 0;
        }

        public CanDiagSendCommand(
            UInt32 inSendCanId,
            IReadOnlyList<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            IReadOnlyList<PublicApis.CanDataMask> inDataMaskList,
            UInt32 inSendNta,
            UInt32 inResponseCanId,
            UInt32 inResponseNta,
            double inRepeatTime,
            string inStop
        )
        {
            _NextState = null;
            _StopState = inStop;

            if (inSendDataList.Count > 4095)
            {
                throw new Exception("CanDiagSendの送信最大サイズは4095[B]です。");
            }
            _SendDataList = inSendDataList;
            _DataMaskList = new List<DataMask>();
            if (inDataMask != null)
            {
                _DataMaskList.Add(new DataMask(inDataMask));
            }
            if (inDataMaskList != null)
            {
                foreach (var dataMask in inDataMaskList)
                {
                    _DataMaskList.Add(new DataMask(dataMask));
                }
            }
            foreach (var msk in _DataMaskList)
            {
                if ((msk.StartIdx + msk.MaskList.Count) > _SendDataList.Count)
                {
                    throw new Exception("DataMask設定エラー。");
                }
            }
            _SendCanId = inSendCanId;
            _SendNta = inSendNta;
            _ResponseCanId = inResponseCanId;
            _ResponseNta = inResponseNta;
            _ResponseDataArrayName = null;
            _RepeatTimeMs = (UInt32)(inRepeatTime * 1000);
            if (_RepeatTimeMs < (2 * PublicConfig.SamplingTimeMSec))
            {
                throw new Exception("時間は" + (2 * PublicConfig.SamplingTimeMSec) + "[ms]以上に設定すること。");
            }
        }

        public void Boot(ControllerState ioState)
        {
            _State = State.WaitStart;
            _TimingMs = _RepeatTimeMs;
            _PrevTimestampMs = ioState.TimestampMs;
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            outFinFlag = false;
            _TimingMs += (ioState.TimestampMs - _PrevTimestampMs);
            _PrevTimestampMs = ioState.TimestampMs;
            switch (_State)
            {
                case State.WaitStart:
                    ProcessWaitStart(ioState);
                    break;
                case State.WaitEnd:
                    ProcessWaitEnd(ioState, ref outFinFlag);
                    break;
                default:
                    outFinFlag = true;
                    break;
            }
            CheckStop(ioState, ref outFinFlag);

            return _NextState;
        }

        public void Finally(ControllerState ioState)
        {
            if (!_TimeoutFlag && !_ErrorSeqFlag && (_ResponseDataArrayName != null))
            {
                int idx = 0;

                foreach (var data in _ResponseDataList)
                {
                    string arrayName = _ResponseDataArrayName + @"[" + idx.ToString() + @"]";

                    ioState.VariableFormulaDict.Remove(arrayName);
                    ioState.VariableDict.Add(arrayName, data);
                    idx++;
                }
            }
        }

        private void ProcessWaitStart(ControllerState inState)
        {
            if (_TimingMs >= _RepeatTimeMs)
            {
                bool isRegistered = CanDiagProtocol.Instance.Register(
                    MakeSendData(inState),
                    _SendCanId,
                    _ResponseCanId,
                    _SendNta,
                    _ResponseNta,
                    inState.TimestampMs
                );
                if (isRegistered)
                {
                    _TimingMs = _TimingMs - _RepeatTimeMs;
                    _State = State.WaitEnd;
                }
            }
        }

        private IReadOnlyList<byte> MakeSendData(ControllerState inState)
        {
            List<Byte> sendDataList = new List<byte>(_SendDataList);

            foreach (var dataMask in _DataMaskList)
            {
                int idx = dataMask.StartIdx;
                UInt64 val = ConvertFormula(inState, dataMask.RefVar);
                int lShift = dataMask.LShift - (8 * dataMask.MaskList.Count) + 8;

                foreach (var msk in dataMask.MaskList)
                {
                    sendDataList[idx] = (Byte)(sendDataList[idx] & ~msk);
                    if (lShift >= 0)
                    {
                        sendDataList[idx] = (Byte)(sendDataList[idx] | ((val << lShift) & msk));
                    }
                    else
                    {
                        sendDataList[idx] = (Byte)(sendDataList[idx] | ((val >> -lShift) & msk));
                    }
                    idx++;
                    lShift = lShift + 8;
                }
            }

            return sendDataList;
        }

        private void ProcessWaitEnd(ControllerState ioState, ref bool outFinFlag)
        {
            bool isFinish = CanDiagProtocol.Instance.GetResult(
                out _ResponseDataList,
                out _TimeoutFlag,
                out _ErrorSeqFlag
            );
            if (isFinish)
            {
                if (_RepeatTimeMs == 0)
                {
                    outFinFlag = true;
                }
                else
                {
                    _State = State.WaitStart;
                }
            }
        }

        private void CheckStop(ControllerState ioState, ref bool ioFinFlag)
        {
            if (_StopState != null)
            {
                if (_StopState == ioState.CurrentState)
                {
                    ioFinFlag = true;
                    if (_State == State.WaitEnd)
                    {
                        CanDiagProtocol.Instance.Initialize();
                    }
                }
            }
        }

        private UInt64 ConvertFormula(ControllerState inState, string inOrgFormula)
        {
            var varNameMatches = _VarNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    try
                    {
                        resultStr = resultStr.Replace(varName, inState.VariableDict[varName].ToString());
                    }
                    catch
                    {
                        throw new Exception("変数" + varName + "が見つかりません。⇒NG");
                    }
                }
            }

            return Convert.ToUInt64(_Dt.Compute(resultStr, ""));
        }
    }
}
