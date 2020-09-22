using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class DcCutCommand : IEmVerifCommand
    {
        private Boolean _OnOff;

        public DcCutCommand(Boolean inOnOff)
        {
            _OnOff = inOnOff;
        }

        public void Boot(ControllerState inState)
        {
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (_OnOff)
            {
                ioState.UserDataToEcuStructure1.DcCutFlag = 1;
            }
            else
            {
                ioState.UserDataToEcuStructure1.DcCutFlag = 0;
            }
            ioState.UserDataToEcuStructure1Update = true;
            outFinFlag = true;

            return null;
        }

        public void Finally(ControllerState inState)
        {
        }
    }
}
