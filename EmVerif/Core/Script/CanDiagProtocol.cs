using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmVerif.Core.Script.Command;

namespace EmVerif.Core.Script
{
    class CanDiagProtocol
    {
        private enum State
        {
            Idle,
            WaitSendSfFf,
            WaitRecvFc,
            WaitSendCf,
            WaitRecvSfFf,
            WaitSendFc,
            WaitRecvCf,
            End
        }

        public static CanDiagProtocol Instance = new CanDiagProtocol();

        private List<Byte> _RestSendDataList;
        private UInt32 _SendCanId;
        private UInt32 _SendNta;
        private Byte _Sid;
        private List<Byte> _ResponseDataList;
        private Boolean _SendTimeoutFlag;
        private Boolean _RecvTimeoutFlag;
        private Boolean _ErrorSeqFlag;
        private UInt32 _ResponseCanId;
        private UInt32 _ResponseNta;
        private UInt32 _BaseTimestampMs;

        private const UInt32 _TimeoutMs = 5000;
        private Byte _StMin;
        private Byte _BlockSize;
        private Byte _SendFrameNumInBlock;
        private Byte _SendFrameNum;
        private Byte _ResponseFrameNum;
        private Int32 _RestResponseDataSize;
        private State _State;

        private Boolean _IsExistSendNta
        {
            get
            {
                Boolean ret;

                if (_SendNta == CanDiagSendCommand.NoValue)
                {
                    ret = false;
                }
                else
                {
                    ret = true;
                }

                return ret;
            }
        }
        private Boolean _IsExistResponse
        {
            get
            {
                Boolean ret;

                if (_ResponseCanId == CanDiagSendCommand.NoValue)
                {
                    ret = false;
                }
                else
                {
                    ret = true;
                }

                return ret;
            }
        }
        private Boolean _IsExistResponseNta
        {
            get
            {
                Boolean ret;

                if (_ResponseNta == CanDiagSendCommand.NoValue)
                {
                    ret = false;
                }
                else
                {
                    ret = true;
                }

                return ret;
            }
        }
        private int _SfCfSendDataMaxSize
        {
            get
            {
                int ret;

                if (_IsExistSendNta)
                {
                    ret = 6;
                }
                else
                {
                    ret = 7;
                }

                return ret;
            }
        }
        private int _FfSendDataSize
        {
            get
            {
                int ret;

                if (_IsExistSendNta)
                {
                    ret = 5;
                }
                else
                {
                    ret = 6;
                }

                return ret;
            }
        }
        private int _FcResponseSize
        {
            get
            {
                int ret;

                if (_IsExistResponseNta)
                {
                    ret = 4;
                }
                else
                {
                    ret = 3;
                }

                return ret;
            }
        }
        private int _SfCfResponseMinSize
        {
            get
            {
                int ret;

                if (_IsExistResponseNta)
                {
                    ret = 3;
                }
                else
                {
                    ret = 2;
                }

                return ret;
            }
        }
        private int _FfResponseDataSize
        {
            get
            {
                int ret;

                if (_IsExistResponseNta)
                {
                    ret = 5;
                }
                else
                {
                    ret = 6;
                }

                return ret;
            }
        }
        private int _ResponseNpciStartPos
        {
            get
            {
                int ret;

                if (_IsExistResponseNta)
                {
                    ret = 1;
                }
                else
                {
                    ret = 0;
                }

                return ret;
            }
        }

        public void Initialize()
        {
            _State = State.Idle;
        }

        public Boolean Register(
            in IReadOnlyList<Byte> inDataList,
            in UInt32 inSendCanId,
            in UInt32 inResponseCanId,
            in UInt32 inSendNta,
            in UInt32 inResponseNta,
            in UInt32 inCurTimestampMs
        )
        {
            Boolean ret = false;

            if (_State == State.Idle)
            {
                _RestSendDataList = new List<byte>(inDataList);
                _Sid = inDataList[0];
                _SendCanId = inSendCanId;
                _SendNta = inSendNta;
                _ResponseDataList = new List<byte>();
                _SendTimeoutFlag = false;
                _RecvTimeoutFlag = false;
                _ErrorSeqFlag = false;
                _ResponseCanId = inResponseCanId;
                _ResponseNta = inResponseNta;
                _BaseTimestampMs = inCurTimestampMs;

                _SendFrameNum = 0;
                _ResponseFrameNum = 0;
                _State = State.WaitSendSfFf;
                ret = true;
            }

            return ret;
        }

        public Boolean GetResult(out List<Byte> outDataList, out Boolean outTimeoutFlag, out Boolean outErrorSeqFlag)
        {
            bool ret;

            outTimeoutFlag = _RecvTimeoutFlag || _SendTimeoutFlag;
            outErrorSeqFlag = _ErrorSeqFlag;
            outDataList = _ResponseDataList;
            if (_State == State.End)
            {
                if (_SendTimeoutFlag)
                {
                    LogManager.Instance.Set("送信タイムアウト発生⇒NG");
                }
                if (_RecvTimeoutFlag)
                {
                    LogManager.Instance.Set("受信タイムアウト発生⇒NG");
                }
                if (_ErrorSeqFlag)
                {
                    LogManager.Instance.Set("シーケンスエラー発生⇒NG");
                }
                ret = true;
                _State = State.Idle;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public void Process(
            in IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList,
            ref UserDataToEcuStructure0 ioUserDataToEcuStructure0,
            UInt32 inCurTimestampMs
        )
        {
            if (inUserDataFromEcuStructureList.Count == 0)
            {
                return;
            }
            switch (_State)
            {
                case State.WaitSendSfFf:
                    WaitSendSfFf(inUserDataFromEcuStructureList, ref ioUserDataToEcuStructure0, inCurTimestampMs);
                    break;
                case State.WaitRecvFc:
                    WaitRecvFc(inUserDataFromEcuStructureList, inCurTimestampMs);
                    break;
                case State.WaitSendCf:
                    WaitSendCf(inUserDataFromEcuStructureList, ref ioUserDataToEcuStructure0, inCurTimestampMs);
                    break;
                case State.WaitRecvSfFf:
                    WaitRecvSfFf(inUserDataFromEcuStructureList, inCurTimestampMs);
                    break;
                case State.WaitSendFc:
                    WaitSendFc(inUserDataFromEcuStructureList, ref ioUserDataToEcuStructure0, inCurTimestampMs);
                    break;
                case State.WaitRecvCf:
                    WaitRecvCf(inUserDataFromEcuStructureList, inCurTimestampMs);
                    break;
                case State.Idle:
                case State.End:
                default:
                    break;
            }
        }

        private void WaitSendSfFf(
            in IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList,
            ref UserDataToEcuStructure0 ioUserDataToEcuStructure0,
            UInt32 inCurTimestampMs
        )
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _SendTimeoutFlag = true;
                _State = State.End;

                return;
            }

            if (ioUserDataToEcuStructure0.CanSendNum < inUserDataFromEcuStructureList.Last().CanSendPossibleNum)
            {
                UInt32 id = ioUserDataToEcuStructure0.CanSendNum;

                ioUserDataToEcuStructure0.CanSendData[id].CanId = _SendCanId;
                ioUserDataToEcuStructure0.CanSendData[id].IsExtendedId = 0;
                ioUserDataToEcuStructure0.CanSendData[id].IsRemoteFrame = 0;

                if (_SfCfSendDataMaxSize >= _RestSendDataList.Count)
                {
                    SetSf(ref ioUserDataToEcuStructure0.CanSendData[id].Data, ref ioUserDataToEcuStructure0.CanSendData[id].DataLen);
                    if (_IsExistResponse)
                    {
                        _State = State.WaitRecvSfFf;
                    }
                    else
                    {
                        _State = State.End;
                    }
                }
                else
                {
                    SetFf(ref ioUserDataToEcuStructure0.CanSendData[id].Data, ref ioUserDataToEcuStructure0.CanSendData[id].DataLen);
                    _State = State.WaitRecvFc;
                }
                _BaseTimestampMs = inCurTimestampMs;
                _SendFrameNum++;
                ioUserDataToEcuStructure0.CanSendNum++;
            }
        }

        private void WaitRecvFc(
            in IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList,
            UInt32 inCurTimestampMs
        )
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _RecvTimeoutFlag = true;
                _State = State.End;

                return;
            }
            int hitCnt = 0;

            foreach (var userDataFromEcu in inUserDataFromEcuStructureList)
            {
                for (int idx = 0; idx < userDataFromEcu.CanRecvNum; idx++)
                {
                    var canRecvData = userDataFromEcu.CanRecvData[idx];

                    if (
                        (canRecvData.CanId == _ResponseCanId) &&
                        (
                            !_IsExistResponseNta ||
                            ((canRecvData.Data[0] == _ResponseNta) && (canRecvData.DataLen >= 1))
                        )
                    )
                    {
                        hitCnt++;
                        if (hitCnt > 1)
                        {
                            _ErrorSeqFlag = true;
                            _State = State.End;

                            return;
                        }
                        else if (
                            (canRecvData.DataLen >= _FcResponseSize) &&
                            ((canRecvData.Data[_ResponseNpciStartPos] & 0xF0) == 0x30)
                        )
                        {
                            _BlockSize = canRecvData.Data[_ResponseNpciStartPos + 1];
                            _StMin = canRecvData.Data[_ResponseNpciStartPos + 2];
                            _BaseTimestampMs = inCurTimestampMs;
                            _State = State.WaitSendCf;
                        }
                        else
                        {
                            _ErrorSeqFlag = true;
                            _State = State.End;

                            return;
                        }
                        _SendFrameNumInBlock = 0;
                    }
                }
            }
        }

        private void WaitSendCf(
            in IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList,
            ref UserDataToEcuStructure0 ioUserDataToEcuStructure0,
            UInt32 inCurTimestampMs
        )
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _SendTimeoutFlag = true;
                _State = State.End;

                return;
            }

            if (
                ((inCurTimestampMs - _BaseTimestampMs) >= _StMin) &&
                (ioUserDataToEcuStructure0.CanSendNum < inUserDataFromEcuStructureList.Last().CanSendPossibleNum)
            )
            {
                UInt32 id = ioUserDataToEcuStructure0.CanSendNum;

                ioUserDataToEcuStructure0.CanSendData[id].CanId = _SendCanId;
                ioUserDataToEcuStructure0.CanSendData[id].IsExtendedId = 0;
                ioUserDataToEcuStructure0.CanSendData[id].IsRemoteFrame = 0;

                SetCf(ref ioUserDataToEcuStructure0.CanSendData[id].Data, ref ioUserDataToEcuStructure0.CanSendData[id].DataLen);

                _SendFrameNum++;
                _SendFrameNumInBlock++;
                ioUserDataToEcuStructure0.CanSendNum++;
                _BaseTimestampMs = inCurTimestampMs;

                if (_RestSendDataList.Count == 0)
                {
                    if (_IsExistResponse)
                    {
                        _State = State.WaitRecvSfFf;
                    }
                    else
                    {
                        _State = State.End;
                    }
                }
                else if ((_BlockSize > 0) && (_BlockSize <= _SendFrameNumInBlock))
                {
                    _State = State.WaitRecvFc;
                }
            }
        }

        private void WaitRecvSfFf(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList, UInt32 inCurTimestampMs)
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _RecvTimeoutFlag = true;
                _State = State.End;

                return;
            }
            int hitCnt = 0;

            foreach (var userDataFromEcu in inUserDataFromEcuStructureList)
            {
                for (int canRecvDataIdx = 0; canRecvDataIdx < userDataFromEcu.CanRecvNum; canRecvDataIdx++)
                {
                    var canRecvData = userDataFromEcu.CanRecvData[canRecvDataIdx];

                    if (
                        (canRecvData.CanId == _ResponseCanId) &&
                        (
                            !_IsExistResponseNta ||
                            ((canRecvData.Data[0] == _ResponseNta) && (canRecvData.DataLen >= 1))
                        )
                    )
                    {
                        hitCnt++;
                        if (hitCnt > 1)
                        {
                            _ErrorSeqFlag = true;
                            _State = State.End;

                            return;
                        }
                        else if (
                            (canRecvData.DataLen >= _SfCfResponseMinSize) &&
                            ((canRecvData.Data[_ResponseNpciStartPos] & 0xF0) == 0x00)
                        )
                        {
                            // SF 受信
                            int dataSize = canRecvData.Data[_ResponseNpciStartPos] & 0x0F;

                            if ((dataSize + _ResponseNpciStartPos + 1) > canRecvData.DataLen)
                            {
                                _ErrorSeqFlag = true;
                                _State = State.End;
                            }
                            else
                            {
                                if (
                                    (dataSize == 3) &&
                                    (canRecvData.Data[_ResponseNpciStartPos + 1] == 0x7F) &&
                                    (canRecvData.Data[_ResponseNpciStartPos + 2] == _Sid) &&
                                    (canRecvData.Data[_ResponseNpciStartPos + 3] == 0x78)
                                )
                                {
                                    // NRC 0x78 受信
                                    hitCnt = 0;
                                    _BaseTimestampMs = inCurTimestampMs;
                                    _State = State.WaitRecvSfFf;
                                }
                                else
                                {
                                    for (int idx = 0; idx < dataSize; idx++)
                                    {
                                        _ResponseDataList.Add(canRecvData.Data[idx + _ResponseNpciStartPos + 1]);
                                    }
                                    _State = State.End;
                                }
                            }
                        }
                        else if (
                            (canRecvData.DataLen >= 8) &&
                            ((canRecvData.Data[_ResponseNpciStartPos] & 0xF0) == 0x10)
                        )
                        {
                            // CF 受信
                            _RestResponseDataSize = (canRecvData.Data[_ResponseNpciStartPos] & 0x0F) * 256 + canRecvData.Data[_ResponseNpciStartPos + 1];
                            if (_RestResponseDataSize <= _FfResponseDataSize)
                            {
                                _ErrorSeqFlag = true;
                                _State = State.End;
                            }
                            else
                            {
                                for (int idx = 0; idx < _FfResponseDataSize; idx++)
                                {
                                    _ResponseDataList.Add(canRecvData.Data[idx + _ResponseNpciStartPos + 2]);
                                    _RestResponseDataSize--;
                                }
                                _ResponseFrameNum++;
                                _BaseTimestampMs = inCurTimestampMs;
                                _State = State.WaitSendFc;
                            }
                        }
                        else
                        {
                            _ErrorSeqFlag = true;
                            _State = State.End;

                            return;
                        }
                    }
                }
            }
        }

        private void WaitSendFc(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList, ref UserDataToEcuStructure0 ioUserDataToEcuStructure0, uint inCurTimestampMs)
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _SendTimeoutFlag = true;
                _State = State.End;

                return;
            }

            if (ioUserDataToEcuStructure0.CanSendNum < inUserDataFromEcuStructureList.Last().CanSendPossibleNum)
            {
                UInt32 id = ioUserDataToEcuStructure0.CanSendNum;

                ioUserDataToEcuStructure0.CanSendData[id].CanId = _SendCanId;
                ioUserDataToEcuStructure0.CanSendData[id].IsExtendedId = 0;
                ioUserDataToEcuStructure0.CanSendData[id].IsRemoteFrame = 0;

                SetFc(ref ioUserDataToEcuStructure0.CanSendData[id].Data, ref ioUserDataToEcuStructure0.CanSendData[id].DataLen);
                _State = State.WaitRecvCf;
                _BaseTimestampMs = inCurTimestampMs;
                ioUserDataToEcuStructure0.CanSendNum++;
            }
        }

        private void WaitRecvCf(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList, uint inCurTimestampMs)
        {
            if ((inCurTimestampMs - _BaseTimestampMs) >= _TimeoutMs)
            {
                _RecvTimeoutFlag = true;
                _State = State.End;

                return;
            }

            foreach (var userDataFromEcu in inUserDataFromEcuStructureList)
            {
                for (int canRecvDataIdx = 0; canRecvDataIdx < userDataFromEcu.CanRecvNum; canRecvDataIdx++)
                {
                    var canRecvData = userDataFromEcu.CanRecvData[canRecvDataIdx];

                    if (
                        (canRecvData.CanId == _ResponseCanId) &&
                        (
                            !_IsExistResponseNta ||
                            ((canRecvData.Data[0] == _ResponseNta) && (canRecvData.DataLen >= 1))
                        )
                    )
                    {
                        if (
                            (canRecvData.DataLen >= _SfCfResponseMinSize) &&
                            ((canRecvData.Data[_ResponseNpciStartPos] & 0xF0) == 0x20) &&
                            ((canRecvData.Data[_ResponseNpciStartPos] & 0x0F) == (_ResponseFrameNum & 0x0F)) &&
                            (_RestResponseDataSize > 0)
                        )
                        {
                            int dataSize = canRecvData.DataLen - _ResponseNpciStartPos - 1;

                            if (dataSize < _RestResponseDataSize)
                            {
                                for (int idx = 0; idx < dataSize; idx++)
                                {
                                    _ResponseDataList.Add(canRecvData.Data[idx + _ResponseNpciStartPos + 1]);
                                }
                                _RestResponseDataSize -= dataSize;
                                _BaseTimestampMs = inCurTimestampMs;
                                _ResponseFrameNum++;
                                _State = State.WaitRecvCf;
                            }
                            else
                            {
                                for (int idx = 0; idx < _RestResponseDataSize; idx++)
                                {
                                    _ResponseDataList.Add(canRecvData.Data[idx + _ResponseNpciStartPos + 1]);
                                }
                                _RestResponseDataSize = 0;
                                _State = State.End;
                                return;
                            }
                        }
                        else
                        {
                            _ErrorSeqFlag = true;
                            _State = State.End;

                            return;
                        }
                    }
                }
            }
        }

        private void SetSf(ref byte[] ioData, ref byte ioDataLen)
        {
            int idx = 0;
            int sendSize = Math.Min(_SfCfSendDataMaxSize, _RestSendDataList.Count);

            if (_IsExistSendNta)
            {
                ioData[idx] = (Byte)(_SendNta & 0xFF);
                idx++;
            }
            ioData[idx] = (Byte)(sendSize & 0x0F);
            idx++;
            foreach (var data in _RestSendDataList.GetRange(0, sendSize))
            {
                ioData[idx] = data;
                idx++;
            }
            _RestSendDataList = new List<byte>();
            // TODO: ８バイト固定のオンオフ切替が必要。
            ioDataLen = 8;
        }

        private void SetFf(ref byte[] ioData, ref byte ioDataLen)
        {
            int idx = 0;

            if (_IsExistSendNta)
            {
                ioData[idx] = (Byte)(_SendNta & 0xFF);
                idx++;
            }
            ioData[idx] = (Byte)(((_RestSendDataList.Count & 0x0F00) >> 8) + 0x10);
            idx++;
            ioData[idx] = (Byte)(_RestSendDataList.Count & 0xFF);
            idx++;
            foreach (var data in _RestSendDataList.GetRange(0, _FfSendDataSize))
            {
                ioData[idx] = data;
                idx++;
            }
            _RestSendDataList.RemoveRange(0, _FfSendDataSize);
            ioDataLen = 8;
        }

        private void SetCf(ref byte[] ioData, ref byte ioDataLen)
        {
            int idx = 0;
            int sendSize = Math.Min(_SfCfSendDataMaxSize, _RestSendDataList.Count);

            if (_IsExistSendNta)
            {
                ioData[idx] = (Byte)(_SendNta & 0xFF);
                idx++;
            }
            ioData[idx] = (Byte)((_SendFrameNum & 0x0F) + 0x20);
            idx++;
            foreach (var data in _RestSendDataList.GetRange(0, sendSize))
            {
                ioData[idx] = data;
                idx++;
            }
            _RestSendDataList.RemoveRange(0, sendSize);

            // TODO: ８バイト固定のオンオフ切替が必要。
            ioDataLen = 8;
        }

        private void SetFc(ref byte[] ioData, ref byte ioDataLen)
        {
            int idx = 0;

            if (_IsExistSendNta)
            {
                ioData[idx] = (Byte)(_SendNta & 0xFF);
                idx++;
            }
            ioData[idx] = (Byte)0x30;
            idx++;
            ioData[idx] = (Byte)0x00;
            idx++;
            ioData[idx] = (Byte)0x02;
            idx++;
            // TODO: ８バイト固定のオンオフ切替が必要。
            ioDataLen = 8;
        }
    }
}
