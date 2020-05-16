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
            InChart.Init("In " + EmVerif.Script.PublicConfig.InChNum + "Ch", EmVerif.Script.PublicConfig.InChNum);
            MixOutChart.Init("MixOut " + EmVerif.Script.PublicConfig.MixOutChNum + "Ch", EmVerif.Script.PublicConfig.MixOutChNum);
            ThroughOutChart.Init("ThroughOut " + EmVerif.Script.PublicConfig.ThroughOutChNum + "Ch", EmVerif.Script.PublicConfig.ThroughOutChNum);
        }

        public void Set(IReadOnlyList<double> inInDataList, IReadOnlyList<double> inMixOutDataList, IReadOnlyList<double> inThroughOutDataList)
        {
            lock(_dataLock)
            {
                InChart.SetData(inInDataList);
                MixOutChart.SetData(inMixOutDataList);
                ThroughOutChart.SetData(inThroughOutDataList);
            }
        }

        public void UpdateGraphs()
        {
            lock(_dataLock)
            {
                InChart.UpdateDataForSync();
                MixOutChart.UpdateDataForSync();
                ThroughOutChart.UpdateDataForSync();
            }
            InChart.UpdateChart();
            MixOutChart.UpdateChart();
            ThroughOutChart.UpdateChart();
        }
    }
}
