using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    public interface IEmVerifCommand
    {
        void Boot(ControllerState inState);
        string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag);
        void Finally();
    }
}
