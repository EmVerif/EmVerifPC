using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class WaitCommand : IEmVerifCommand
    {
        private string _NextState;
        private UInt32 _WaitTimeMSec;
        private UInt32 _BootTimeMSec;

        public WaitCommand(double inWaitTimeSec, string inNext)
        {
            double waitTimeMSec = inWaitTimeSec * 1000;
            if (waitTimeMSec < 0)
            {
                waitTimeMSec = 0;
            }
            _NextState = inNext;
            _WaitTimeMSec = (UInt32)waitTimeMSec;
            _BootTimeMSec = 0;
        }

        public void Boot(ControllerState inState)
        {
            _BootTimeMSec = inState.TimestampMs;
        }

        public string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag)
        {
            if ((ioState.TimestampMs - _BootTimeMSec) >= _WaitTimeMSec)
            {
                outFinFlag = true;
            }
            else
            {
                outFinFlag = false;
            }

            return _NextState;
        }

        public void Finally(ControllerState inState)
        {
        }
    }
}
