using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Script.Command
{
    public interface ICommand
    {
        void Boot(ControllerState inState);
        string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag);
        void Finally();
    }
}
