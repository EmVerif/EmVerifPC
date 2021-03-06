﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using EmVerif.Core.Utility;

namespace EmVerif.Core.Script.Command
{
    class CanSendCommand : IEmVerifCommand
    {
        public const UInt32 NoValue = 0xFFFFFFFF;

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
                if (((dataMask.BitLen + LShift) > 64) || (dataMask.BitPos >= 8))
                {
                    throw new Exception("DataMask設定エラー。");
                }
            }
        }

        private string _NextState;
        private string _StopState;

        private UInt32 _SendCanId;
        private UInt64 _SendData;
        private Int32 _SendDataLen;
        private List<DataMask> _DataMaskList;
        private UInt32 _ResponseCanId;
        private UInt32 _RepeatTimeMs;

        private State _State;
        private UInt32 _PrevTimestampMs;
        private UInt32 _TimingMs;

        public CanSendCommand(
            UInt32 inSendCanId,
            IReadOnlyList<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            IReadOnlyList<PublicApis.CanDataMask> inDataMaskList,
            UInt32 inResponseCanId,
            string inNext
        )
        {
            _NextState = inNext;
            _StopState = null;
            _SendCanId = inSendCanId;
            _SendData = ConvertToUInt64(inSendDataList);
            _SendDataLen = inSendDataList.Count;
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
                if ((msk.LShift + (_SendDataLen * 8)) < 64)
                {
                    throw new Exception("DataMask設定エラー。");
                }
            }
            _ResponseCanId = inResponseCanId;
            _RepeatTimeMs = 0;
            CheckParam();
        }

        public CanSendCommand(
            UInt32 inSendCanId,
            IReadOnlyList<Byte> inSendDataList,
            PublicApis.CanDataMask inDataMask,
            List<PublicApis.CanDataMask> inDataMaskList,
            UInt32 inResponseCanId,
            double inRepeatTime,
            string inStop
        )
        {
            _NextState = null;
            _StopState = inStop;
            _SendCanId = inSendCanId;
            _SendData = ConvertToUInt64(inSendDataList);
            _SendDataLen = inSendDataList.Count;
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
                if ((msk.LShift + (_SendDataLen * 8)) < 64)
                {
                    throw new Exception("DataMask設定エラー。");
                }
            }
            _ResponseCanId = inResponseCanId;
            _RepeatTimeMs = (UInt32)(inRepeatTime * 1000);
            if (_RepeatTimeMs < (2 * PublicConfig.SamplingTimeMSec))
            {
                throw new Exception("時間は" + (2 * PublicConfig.SamplingTimeMSec) + "[ms]以上に設定すること。");
            }
            CheckParam();
        }

        public void Boot(ControllerState ioState)
        {
            _State = State.WaitSend;
            _TimingMs = _RepeatTimeMs;
            _PrevTimestampMs = ioState.TimestampMs;
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

        public void Finally(ControllerState inState)
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
            if ((_ResponseCanId > 0x20000000) && (_ResponseCanId != NoValue))
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
                if (_ResponseCanId == NoValue)
                {
                    if (_RepeatTimeMs == 0)
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
                if (_RepeatTimeMs == 0)
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
            if (_StopState != null)
            {
                if (_StopState == ioState.CurrentState)
                {
                    ioFinFlag = true;
                }
            }
        }

        private Boolean CheckSend(ControllerState ioState)
        {
            Boolean ret = false;

            if (ioState.UserDataFromEcuStructureList.Count != 0)
            {
                UInt32 sendPossibleNum = ioState.UserDataFromEcuStructureList.Last().CanSendPossibleNum;
                UInt32 sendNum = ioState.UserDataToEcuStructure0.CanSendNum;

                if ((sendPossibleNum > sendNum) && (_TimingMs >= _RepeatTimeMs))
                {
                    UInt64 sendData = _SendData;

                    foreach (var dataMask in _DataMaskList)
                    {
                        sendData = ConvertFormula(ioState, dataMask.RefVar, sendData, dataMask.Mask, dataMask.LShift);
                    }
                    ioState.UserDataToEcuStructure0.CanSendData[sendNum].CanId = _SendCanId;
                    ioState.UserDataToEcuStructure0.CanSendData[sendNum].IsExtendedId = 0;
                    ioState.UserDataToEcuStructure0.CanSendData[sendNum].IsRemoteFrame = 0;
                    ioState.UserDataToEcuStructure0.CanSendData[sendNum].Data = ConvertTo8ByteArray(sendData);
                    ioState.UserDataToEcuStructure0.CanSendData[sendNum].DataLen = (Byte)_SendDataLen;
                    ioState.UserDataToEcuStructure0.CanSendNum++;
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

        private UInt64 ConvertToUInt64(IReadOnlyList<Byte> inDataList)
        {
            UInt64 ret = 0;
            Int32 lShift = 56;

            if (inDataList.Count <= 8)
            {
                foreach (var data in inDataList)
                {
                    ret = ret | ((UInt64)data << lShift);
                    lShift = lShift - 8;
                }
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

        private UInt64 ConvertFormula(ControllerState inState, string inOrgFormula, UInt64 inOrgData, UInt64 inMask, Int32 inLShift)
        {
            return ((Convert.ToUInt64(Calculator.Instance.ConvertFormula(inOrgFormula, inState.VariableDict)) << inLShift) & inMask) | (inOrgData & ~inMask);
        }
    }
}
