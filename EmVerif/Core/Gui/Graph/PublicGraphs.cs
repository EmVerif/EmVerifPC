using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmVerif.Core.Gui.Graph
{
    public partial class PublicGraphs : UserControl
    {
        private object _dataLock = new object();

        public PublicGraphs()
        {
            InitializeComponent();
            AdChart.Init("AD " + EmVerif.Core.Script.PublicConfig.AdChNum + "Ch", EmVerif.Core.Script.PublicConfig.AdChNum);
            PwmChart.Init("PWM " + EmVerif.Core.Script.PublicConfig.PwmChNum + "Ch", EmVerif.Core.Script.PublicConfig.PwmChNum);
            SpioutChart.Init("SPIOut " + EmVerif.Core.Script.PublicConfig.SpioutChNum + "Ch", EmVerif.Core.Script.PublicConfig.SpioutChNum);
        }

        public void Set(IReadOnlyList<double> inAdDataList, IReadOnlyList<double> inPwmDataList, IReadOnlyList<double> inSpioutDataList)
        {
            lock(_dataLock)
            {
                AdChart.SetData(inAdDataList);
                PwmChart.SetData(inPwmDataList);
                SpioutChart.SetData(inSpioutDataList);
            }
        }

        public void UpdateGraphs()
        {
            lock(_dataLock)
            {
                AdChart.UpdateDataForSync();
                PwmChart.UpdateDataForSync();
                SpioutChart.UpdateDataForSync();
            }
            AdChart.UpdateChart();
            PwmChart.UpdateChart();
            SpioutChart.UpdateChart();
        }
    }
}
