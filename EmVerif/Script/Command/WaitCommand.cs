using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Script.Command
{
    class WaitCommand : ICommand
    {
        private string _NextState;
        private UInt32 _WaitTimeMSec;
        private UInt32 _BootTimeMSec;

        public WaitCommand(double inWaitTimeSec, string inNextState)
        {
            if (inWaitTimeSec < 0)
            {
                inWaitTimeSec = 0;
            }
            _NextState = inNextState;
            _WaitTimeMSec = (UInt32)(inWaitTimeSec * 1000);
            _BootTimeMSec = 0;
        }

        public void Boot(ControllerState inState)
        {
            _BootTimeMSec = inState.TimestampMs;
        }

        public string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag)
        {
            string retState = null;

            if ((ioState.TimestampMs - _BootTimeMSec) >= _WaitTimeMSec)
            {
                outFinFlag = true;
                retState = _NextState;
            }
            else
            {
                outFinFlag = false;
            }

            return retState;
        }

        public void Finally()
        {
        }
    }
}
