using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmVerif.Gui.Graph
{
    public partial class PublicGraphs : UserControl
    {
        private object _dataLock = new object();

        public PublicGraphs()
        {
            InitializeComponent();
            In6ChChart.Init("In " + EmVerif.Script.PublicConfig.InChNum + "Ch", EmVerif.Script.PublicConfig.InChNum);
            MixOut4ChChart.Init("MixOut " + EmVerif.Script.PublicConfig.MixOutChNum + "Ch", EmVerif.Script.PublicConfig.MixOutChNum);
            ThroughOut12ChChart.Init("ThroughOut " + EmVerif.Script.PublicConfig.ThroughOutChNum + "Ch", EmVerif.Script.PublicConfig.ThroughOutChNum);
        }

        public void Set(IReadOnlyList<double> inInDataList, IReadOnlyList<double> inMixOutDataList, IReadOnlyList<double> inThroughOutDataList)
        {
            lock(_dataLock)
            {
                In6ChChart.SetData(inInDataList);
                MixOut4ChChart.SetData(inMixOutDataList);
                ThroughOut12ChChart.SetData(inThroughOutDataList);
            }
        }

        public void UpdateGraphs()
        {
            lock(_dataLock)
            {
                In6ChChart.UpdateDataForSync();
                MixOut4ChChart.UpdateDataForSync();
                ThroughOut12ChChart.UpdateDataForSync();
            }
            In6ChChart.UpdateChart();
            MixOut4ChChart.UpdateChart();
            ThroughOut12ChChart.UpdateChart();
        }
    }
}
