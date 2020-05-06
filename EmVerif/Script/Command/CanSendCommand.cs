using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmVerif.Script.Command
{
    class CanSendCommand : ICommand
    {
        public const UInt32 NoResponseValue = 0xFFFFFFFF;

        private enum Mode
        {
            OneShot,
            Repeat
        }

        private enum State
        {
            WaitResponse,
            WaitSend
        }

        private class DataMask
        {
            public UInt64 Mask;
            public Int32 LShift;
            public string RefVar;

            public DataMask(PublicApis.CanDataMask dataMask)
            {
                LShift = (Int32)(8 * (7 - dataMask.BytePos) + dataMask.BitPos);
                RefVar = dataMask.RefVar;
                Mask = (((UInt64)1 << (Int32)dataMask.BitLen) - 1) << LShift;
            }
        }

        private string _NextState;
        private string _StopState;
        private UInt32 _SendCanId;
        private UInt64 _SendData;
        private Int32 _SendDataLen;
        private DataMask _DataMask;
        private UInt32 _ResponseCanId;
        private UInt32 _RepeatTimeMs;

        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_]\w*)");
        private State _State;
        private Mode _Mode;
        private UInt32 _PrevTimestampMs;
        private UInt32 _TimingMs;

        public CanSendCommand(
            UInt32 inSendCanId,
            List<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            UInt32 inResponseCanId,
            string inNextState
        )
        {
            _NextState = inNextState;
            _StopState = null;
            _SendCanId = inSendCanId;
            _SendData = ConvertToUInt64(inSendDataList);
            _SendDataLen = inSendDataList.Count;
            if (inDataMask != null)
            {
                _DataMask = new DataMask(inDataMask);
            }
            else
            {
                _DataMask = null;
            }
            _ResponseCanId = inResponseCanId;
            _RepeatTimeMs = 0;
            _Mode = Mode.OneShot;
            CheckParam();
        }

        public CanSendCommand(
            UInt32 inSendCanId,
            List<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            UInt32 inResponseCanId,
            double inRepeatTime,
            string inStopState
        )
        {
            _NextState = null;
            _StopState = inStopState;
            _SendCanId = inSendCanId;
            _SendData = ConvertToUInt64(inSendDataList);
            _SendDataLen = inSendDataList.Count;
            if (inDataMask != null)
            {
                _DataMask = new DataMask(inDataMask);
            }
            else
            {
                _DataMask = null;
            }
            _ResponseCanId = inResponseCanId;
            _RepeatTimeMs = (UInt32)(inRepeatTime * 1000);
            _Mode = Mode.Repeat;
            CheckParam();
        }

        public void Boot(ControllerState inState)
        {
            _State = State.WaitSend;
            _TimingMs = _RepeatTimeMs;
            _PrevTimestampMs = inState.TimestampMs;
        }

        public string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag)
        {
            outFinFlag = false;
            _TimingMs += (ioState.TimestampMs - _PrevTimestampMs);
            _PrevTimestampMs = ioState.TimestampMs;
            switch (_State)
            {
                case State.WaitSend:
                    ProcessWaitSend(ioState, ref outFinFlag);
                    break;
                case State.WaitResponse:
                    ProcessWaitResponse(ioState, ref outFinFlag);
                    break;
                default:
                    outFinFlag = true;
                    break;
            }
            CheckStop(ioState, ref outFinFlag);

            return _NextState;
        }

        public void Finally()
        {
        }

        private void CheckParam()
        {
            if (_SendCanId >= 0x20000000)
            {
                throw new Exception(
                    "CanSend/CanRepSend コマンド内 SendCanId 設定エラー⇒" + _SendCanId + "\n" +
                    "SendCanId 設定値範囲は 0 以上 0x20000000 未満。"
                );
            }
            if (_SendDataLen > 8)
            {
                throw new Exception(
                    "CanSend/CanRepSend コマンド内 SendData 設定エラー\n" +
                    "SendData 設定数は８バイトまで。"
                );
            }
            // inDataMaskList チェックは、DataMask 内で実行。
            if ((_ResponseCanId > 0x20000000) && (_ResponseCanId != NoResponseValue))
            {
                throw new Exception(
                    "CanSend/CanRepSend コマンド内 ResponseCanId 設定エラー⇒" + _ResponseCanId + "\n" +
                    "ResponseCanId 設定値範囲は 0 以上 0x20000000 未満。"
                );
            }
        }

        private void ProcessWaitSend(ControllerState ioState, ref Boolean ioFinFlag)
        {
            if (CheckSend(ioState))
            {
                if (_ResponseCanId == NoResponseValue)
                {
                    if (_Mode == Mode.OneShot)
                    {
                        ioFinFlag = true;
                    }
                }
                else
                {
                    _State = State.WaitResponse;
                }
            }
        }

        private void ProcessWaitResponse(ControllerState ioState, ref Boolean ioFinFlag)
        {
            if (CheckResponse(ioState))
            {
                if (_Mode == Mode.OneShot)
                {
                    ioFinFlag = true;
                }
                else
                {
                    _State = State.WaitSend;
                }
            }
        }

        private void CheckStop(ControllerState ioState, ref bool ioFinFlag)
        {
            if ((_Mode == Mode.Repeat) && (_StopState == ioState.CurrentState))
            {
                ioFinFlag = true;
            }
        }

        private Boolean CheckSend(ControllerState ioState)
        {
            Boolean ret = false;

            if (ioState.UserDataFromEcuStructureList.Count != 0)
            {
                UInt32 sendPossibleNum = ioState.UserDataFromEcuStructureList.Last().CanSendPossibleNum;
                UInt32 sendNum = ioState.UserDataToEcuStructure.CanSendNum;

                if ((sendPossibleNum > sendNum) && (_TimingMs >= _RepeatTimeMs))
                {
                    UInt64 sendData = _SendData;

                    if (_DataMask != null)
                    {
                        sendData = ConvertVariable(ioState, _DataMask.RefVar, sendData, _DataMask.Mask, _DataMask.LShift);
                    }
                    ioState.UserDataToEcuStructure.CanSendData[sendNum].CanId = _SendCanId;
                    ioState.UserDataToEcuStructure.CanSendData[sendNum].IsExtendedId = 0;
                    ioState.UserDataToEcuStructure.CanSendData[sendNum].IsRemoteFrame = 0;
                    ioState.UserDataToEcuStructure.CanSendData[sendNum].Data = ConvertTo8ByteArray(sendData);
                    ioState.UserDataToEcuStructure.CanSendData[sendNum].DataLen = (Byte)_SendDataLen;
                    ioState.UserDataToEcuStructure.CanSendNum++;
                    _TimingMs = _TimingMs - _RepeatTimeMs;
                    ret = true;
                }
            }

            return ret;
        }

        private Boolean CheckResponse(ControllerState ioState)
        {
            Boolean foundFlag = false;

            foreach (var userDataFromEcuStructure in ioState.UserDataFromEcuStructureList)
            {
                for (UInt32 idx = 0; idx < userDataFromEcuStructure.CanRecvNum; idx++)
                {
                    if (_ResponseCanId == userDataFromEcuStructure.CanRecvData[idx].CanId)
                    {
                        foundFlag = true;
                    }
                }
            }

            return foundFlag;
        }

        private UInt64 ConvertToUInt64(List<Byte> inDataList)
        {
            UInt64 ret = 0;
            Int32 lShift = 56;

            if (inDataList.Count <= 8)
            {
                inDataList.ForEach(data => { ret = ret | ((UInt64)data << lShift); lShift = lShift - 8; });
            }

            return ret;
        }

        private Byte[] ConvertTo8ByteArray(UInt64 inData)
        {
            List<Byte> retList = new List<Byte>();

            for (int idx = 0; idx < 8; idx++)
            {
                retList.Add((Byte)((inData >> (56 - 8 * idx)) & 0xFF));
            }

            return retList.ToArray();
        }

        private UInt64 ConvertVariable(ControllerState ioState, string inOrgFormula, UInt64 inOrgData, UInt64 inMask, Int32 inLShift)
        {
            DataTable dt = new DataTable();
            var varNameMatches = _VarNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    resultStr = resultStr.Replace(varName, ioState.VariableDict[varName].ToString());
                }
            }

            return ((Convert.ToUInt64(dt.Compute(resultStr, "")) << inLShift) & inMask) | (inOrgData & ~inMask);
        }
    }
}
