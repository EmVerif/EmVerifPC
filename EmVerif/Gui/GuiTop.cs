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
        public GuiTop()
        {
            InitializeComponent();
        }

        public void SetGraph(List<double> inInDataList, List<double> inMixOutDataList, List<double> inThroughOutDataList)
        {
            publicGraphs1.Set(inInDataList, inMixOutDataList, inThroughOutDataList);
        }

        public void SetLoadBar(List<double> inDataList)
        {
            publicLoadBar1.Set(inDataList);
        }
    }
}
