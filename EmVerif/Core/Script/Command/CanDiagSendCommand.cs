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

        private State _State;
        private string _Next;

        private IReadOnlyList<Byte> _SendDataList;
        private UInt32 _SendCanId;
        private UInt32 _SendNta;
        private UInt32 _ResponseCanId;
        private UInt32 _ResponseNta;
        private string _ResponseDataArrayName;
        private List<Byte> _ResponseDataList;
        private Boolean _TimeoutFlag;
        private Boolean _ErrorSeqFlag;

        public CanDiagSendCommand(
            string inNext,
            IReadOnlyList<Byte> inSendData,
            UInt32 inSendCanId,
            UInt32 inSendNta,
            UInt32 inResponseCanId,
            UInt32 inResponseNta,
            string inResponseDataArrayName
        )
        {
            _Next = inNext;

            if (inSendData.Count > 4095)
            {
                throw new Exception("CanDiagSendの送信最大サイズは4095[B]。");
            }
            _SendDataList = inSendData;
            _SendCanId = inSendCanId;
            _SendNta = inSendNta;
            _ResponseCanId = inResponseCanId;
            _ResponseNta = inResponseNta;
            _ResponseDataArrayName = inResponseDataArrayName;
        }

        public void Boot(ControllerState ioState)
        {
            _State = State.WaitStart;
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            outFinFlag = false;
            switch (_State)
            {
                case State.WaitStart:
                    bool isRegistered = CanDiagProtocol.Instance.Register(
                        _SendDataList,
                        _SendCanId,
                        _ResponseCanId,
                        _SendNta,
                        _ResponseNta
                    );
                    if (isRegistered)
                    {
                        _State = State.WaitEnd;
                    }
                    break;
                case State.WaitEnd:
                    bool isFinish = CanDiagProtocol.Instance.GetResult(
                        out _ResponseDataList,
                        out _TimeoutFlag,
                        out _ErrorSeqFlag
                    );
                    if (isFinish)
                    {
                        outFinFlag = true;
                    }
                    break;
                default:
                    outFinFlag = true;
                    break;
            }

            return _Next;
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
