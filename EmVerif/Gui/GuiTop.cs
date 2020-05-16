using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EmVerif.Communication;

namespace EmVerif.Gui
{
    public partial class GuiTop : Form
    {
        public Boolean FormClosingRequest;

        public GuiTop()
        {
            InitializeComponent();
            FormClosingRequest = false;
            tm_Update.Interval = 100;
            tm_Update.Start();
        }

        public void SetGraph(IReadOnlyList<double> inInDataList, IReadOnlyList<double> inMixOutDataList, IReadOnlyList<double> inThroughOutDataList)
        {
            publicGraphs1.Set(inInDataList, inMixOutDataList, inThroughOutDataList);
        }

        public void SetLoadBar(IReadOnlyList<double> inDataList)
        {
            publicLoadBar1.Set(inDataList);
        }

        public void SetVariable(Dictionary<string, Decimal> inNameToValueDic, Dictionary<string, string> inNameToFormulaDic)
        {
            publicVariableView1.Set(inNameToValueDic, inNameToFormulaDic);
        }

        public void SetLog(string inLog)
        {
            publicLog1.SetLog(inLog);
        }

        public void SetState(string inState)
        {
            publicLog1.SetState(inState);
        }

        public IReadOnlyDictionary<string, Decimal> GetUpdatedValue()
        {
            return publicVariableView1.GetUpdatedValue();
        }

        private void tm_Update_Tick(object sender, EventArgs e)
        {
            publicGraphs1.UpdateGraphs();
            publicLoadBar1.UpdateLoadBar();
            publicVariableView1.UpdateVariableView();
            publicLog1.UpdateLog();
        }

        private void GuiTop_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormClosingRequest = true;
            e.Cancel = true;
        }
    }
}
