using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmVerif.Gui.Log
{
    public partial class PublicLog : UserControl
    {
        private Int32 _MaxLogCount = 100;
        private object _Lock = new object();
        private List<string> _LogList = new List<string>();
        private string _CurrentState = "";

        public PublicLog()
        {
            InitializeComponent();
        }

        public void SetLog(string inLog)
        {
            lock (_Lock)
            {
                _LogList.Add(inLog);
                if (_LogList.Count > _MaxLogCount)
                {
                    _LogList.RemoveRange(0, _LogList.Count - _MaxLogCount);
                }
            }
        }

        public void SetState(string inState)
        {
            lock (_Lock)
            {
                _CurrentState = inState;
            }
        }

        public void UpdateLog()
        {
            string logs = "";

            lock (_Lock)
            {
                _LogList.ForEach(log => logs += log + Environment.NewLine);
                textBox2.Text = "Current State = " + _CurrentState;
            }
            textBox1.Text = logs;
            textBox1.SelectionStart = logs.Length;
            textBox1.ScrollToCaret();
        }
    }
}
