using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class FadeCommand : IEmVerifCommand
    {
        public FadeCommand(
            double inTimeSec,
            string inNextState
        )
        {

        }

        public void Boot(ControllerState inState)
        {
            throw new NotImplementedException();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            throw new NotImplementedException();
        }

        public void Finally()
        {
            throw new NotImplementedException();
        }
    }
}
