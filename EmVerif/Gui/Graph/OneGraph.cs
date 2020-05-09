using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace EmVerif.Gui.Graph
{
    public partial class OneGraph : UserControl
    {
        public const Int32 ChartSmpNum = 250;

        private DataSet _DataSet;
        private Int32 _ChNum;

        private List<double> _SetDataList = new List<double>();
        private Boolean _SetDataListUpdate = false;
        private IReadOnlyList<double> _DataForSyncList = new List<double>();

        public OneGraph()
        {
            InitializeComponent();
        }

        public void Init(string inTitle, Int32 inChNum)
        {
            _ChNum = inChNum;
            chart_FastLine.Titles.Add(inTitle);
            GenerateData();
            SetChart();
            GenerateCheckbox();
        }

        #region 別タスク、関数コール側で排他制御必要
        public void SetData(IReadOnlyList<double> inDataList)
        {
            Int32 count;

            _SetDataList.AddRange(inDataList);
            count = _SetDataList.Count;
            if (count > _ChNum * ChartSmpNum)
            {
                _SetDataList.RemoveRange(0, count - (_ChNum * ChartSmpNum));
            }
            _SetDataListUpdate = true;
        }
        #endregion

        #region 関数コール側で排他制御必要
        public void UpdateDataForSync()
        {
            if(_SetDataListUpdate)
            {
                _DataForSyncList = new List<double>(_SetDataList);
            }
            _SetDataListUpdate = false;
        }
        #endregion

        public void UpdateChart()
        {
            Int32 col = 0;
            Int32 row = 1;

            foreach (double data in _DataForSyncList)
            {
                _DataSet.Tables[0].Rows[col][row] = data;
                row++;
                if (row > _ChNum)
                {
                    row = 1;
                    col++;
                }
            }
            chart_FastLine.DataBind();
        }

        private void GenerateData()
        {
            DataTable dt = new DataTable();
            DataRow dtRow;

            _DataSet = new DataSet();
            dt.Columns.Add("時間[ms]", Type.GetType("System.Double"));
            for (Int32 idx = 0; idx < _ChNum; idx++)
            {
                dt.Columns.Add("Ch" + idx.ToString(), Type.GetType("System.Double"));
            }
            _DataSet.Tables.Add(dt);

            // データの追加
            for (Int32 idx = 0; idx < ChartSmpNum; idx++)
            {
                dtRow = _DataSet.Tables[0].NewRow();
                dtRow[0] = ((double)idx / EmVerif.Communication.PublicConfig.SamplingKhz);
                _DataSet.Tables[0].Rows.Add(dtRow);
            }
        }

        private void SetChart()
        {
            chart_FastLine.ChartAreas[0].AxisX.Minimum = 0;
            chart_FastLine.ChartAreas[0].AxisX.Maximum = ChartSmpNum / EmVerif.Communication.PublicConfig.SamplingKhz;
            chart_FastLine.ChartAreas[0].AxisY.Minimum = -1;
            chart_FastLine.ChartAreas[0].AxisY.Maximum = 1;
            chart_FastLine.DataSource = _DataSet;
            for (Int32 i = 1; i < _DataSet.Tables[0].Columns.Count; i++)
            {
                string columnName = _DataSet.Tables[0].Columns[i].ColumnName;

                chart_FastLine.Series.Add(columnName);
                chart_FastLine.Series[columnName].ChartType = SeriesChartType.FastLine;
                chart_FastLine.Series[columnName].XValueMember = _DataSet.Tables[0].Columns[0].ColumnName;
                chart_FastLine.Series[columnName].YValueMembers = columnName;
            }
            chart_FastLine.DataBind();
        }

        private void GenerateCheckbox()
        {
            for (Int32 idx = 0; idx < _ChNum; idx++)
            {
                CheckBox cb = new CheckBox();

                cb.Text = "Ch" + idx.ToString();
                cb.Width = 50;
                cb.Checked = true;
                cb.CheckedChanged += Cb_CheckedChangedEvent;
                flowLayoutPanel1.Controls.Add(cb);
            }
        }

        #region イベント
        private void Cb_CheckedChangedEvent(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                chart_FastLine.Series[((CheckBox)sender).Text].Enabled = true;
            }
            else
            {
                chart_FastLine.Series[((CheckBox)sender).Text].Enabled = false;
            }
        }
        #endregion
    }
}
