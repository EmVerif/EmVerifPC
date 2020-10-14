using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public const UInt32 NoValue = 0xFFFFFFFF;

        private string _NextState;
        private string _StopState;

        private IReadOnlyList<Byte> _SendDataList;
        private UInt32 _SendCanId;
        private UInt32 _SendNta;
        private UInt32 _ResponseCanId;
        private UInt32 _ResponseNta;
        private string _ResponseDataArrayName;
        private UInt32 _RepeatTimeMs;

        private List<Byte> _ResponseDataList;
        private Boolean _TimeoutFlag;
        private Boolean _ErrorSeqFlag;

        private State _State;
        private UInt32 _PrevTimestampMs;
        private UInt32 _TimingMs;

        public CanDiagSendCommand(
            string inNext,
            IReadOnlyList<Byte> inSendDataList,
            UInt32 inSendCanId,
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
                throw new Exception("CanDiagSendの送信最大サイズは4095[B]。");
            }
            _SendDataList = inSendDataList;
            _SendCanId = inSendCanId;
            _SendNta = inSendNta;
            _ResponseCanId = inResponseCanId;
            _ResponseNta = inResponseNta;
            _ResponseDataArrayName = inResponseDataArrayName;
            _RepeatTimeMs = 0;
        }

        public CanDiagSendCommand(
            IReadOnlyList<Byte> inSendDataList,
            UInt32 inSendCanId,
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
                throw new Exception("CanDiagSendの送信最大サイズは4095[B]。");
            }
            _SendDataList = inSendDataList;
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

        private void ProcessWaitStart(ControllerState ioState)
        {
            if (_TimingMs >= _RepeatTimeMs)
            {
                bool isRegistered = CanDiagProtocol.Instance.Register(
                    _SendDataList,
                    _SendCanId,
                    _ResponseCanId,
                    _SendNta,
                    _ResponseNta,
                    ioState.TimestampMs
                );
                if (isRegistered)
                {
                    _TimingMs = _TimingMs - _RepeatTimeMs;
                    _State = State.WaitEnd;
                }
            }
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
    }
}
