using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmVerif.Gui;

namespace EmVerif.Script
{
    class LogManager
    {
        public static LogManager Instance = new LogManager();

        private GuiTop _RefGuiTop;
        private StreamWriter _Writer = null;

        public void Start(string inFileName, GuiTop inGuiTop)
        {
            Stop();
            Encoding enc = Encoding.GetEncoding("Shift_JIS");

            _Writer = new StreamWriter(inFileName, false, enc);
            _RefGuiTop = inGuiTop;
        }

        public void Set(string inLog)
        {
            if (_Writer != null)
            {
                _Writer.WriteLine(inLog);
            }
            if (_RefGuiTop != null)
            {
                _RefGuiTop.SetLog(inLog);
            }
        }

        public void Stop()
        {
            if (_Writer != null)
            {
                _Writer.Close();
                _Writer = null;
                _RefGuiTop = null;
            }
        }
    }
}
