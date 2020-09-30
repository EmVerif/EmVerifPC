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

        private List<Byte> _SendDataList;
        private UInt32 _SendCanId;
        private UInt32 _SendNta;
        private List<Byte> _ResponseDataList;
        private Boolean _TimeoutFlag;
        private UInt32 _ResponseCanId;
        private UInt32 _ResponseNta;

        private int _SfMaxSendSize
        {
            get
            {
                int ret;

                if (_SendNta == CanDiagSendCommand.NoValue)
                {
                    ret = 7;
                }
                else
                {
                    ret = 6;
                }

                return ret;
            }
        }

        private State _State;

        public void Initialize()
        {
            _State = State.Idle;
        }

        public Boolean Register(
            in IReadOnlyList<Byte> inDataList,
            in UInt32 inSendCanId,
            in UInt32 inResponseCanId,
            in UInt32 inSendNta,
            in UInt32 inResponseNta
        )
        {
            Boolean ret = false;

            if (_State == State.Idle)
            {
                _SendDataList = new List<byte>(inDataList);
                _SendCanId = inSendCanId;
                _SendNta = inSendNta;
                _ResponseDataList = new List<byte>();
                _TimeoutFlag = false;
                _ResponseCanId = inResponseCanId;
                _ResponseNta = inResponseNta;
                _State = State.WaitSendSfFf;
                ret = true;
            }

            return ret;
        }

        public Boolean GetResult(out List<Byte> outDataList, out Boolean outTimeoutFlag)
        {
            bool ret;

            outTimeoutFlag = _TimeoutFlag;
            outDataList = _ResponseDataList;

            if (_State == State.End)
            {
                ret = true;
                _State = State.Idle;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public void Process(ref UserDataToEcuStructure0 ioUserDataToEcuStructure0)
        {
            switch (_State)
            {
                case State.WaitSendSfFf:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.WaitRecvFc:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.WaitSendCf:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.WaitRecvSfFf:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.WaitSendFc:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.WaitRecvCf:
                    // TODO:
                    throw new NotImplementedException();
                    break;
                case State.Idle:
                case State.End:
                default:
                    break;
            }
        }
    }
}
