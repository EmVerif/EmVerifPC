using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class ExecBatchCommand : IEmVerifCommand
    {
        private string _NextState;

        private ProcessStartInfo _ProcessStartInfo;
        private Process _Process;

        public ExecBatchCommand(
            string inCmd,
            string inNext
        )
        {
            _NextState = inNext;
            _ProcessStartInfo = new ProcessStartInfo("cmd.exe", "/c " + inCmd);
            _ProcessStartInfo.CreateNoWindow = false;
            _ProcessStartInfo.UseShellExecute = true;
        }

        public void Boot(ControllerState inState)
        {
            _Process = Process.Start(_ProcessStartInfo);
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (_Process.HasExited)
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
            if (!_Process.HasExited)
            {
                _Process.Kill();
            }
        }
    }
}
