﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace EmVerif.Core.Gui.Graph
{
    public partial class PublicLoadBar : UserControl
    {
        private DataSet _DataSet;

        private List<double> _DataList = null;
        private Boolean _DataListUpdate = false;
        private object _DataLock = new object();
        private DataTable _Dt = new DataTable();

        public PublicLoadBar()
        {
            InitializeComponent();
            GenerateData();
            SetChart();
        }

        public void Set(IReadOnlyList<double> inDataList)
        {
            if (inDataList.Count == 2)
            {
                lock (_DataLock)
                {
                    _DataList = new List<double>(inDataList);
                    _DataListUpdate = true;
                }
            }
        }

        public void UpdateLoadBar()
        {
            Int32 col = 0;
            List<double> dataList = new List<double>();
            Boolean dataListUpdate;

            lock (_DataLock)
            {
                if (_DataListUpdate)
                {
                    dataList = _DataList;
                }
                dataListUpdate = _DataListUpdate;
                _DataListUpdate = false;
            }
            if (dataListUpdate)
            {
                foreach (double data in dataList)
                {
                    _DataSet.Tables[0].Rows[col][1] = data;
                    col++;
                }
                chart_Bar.DataBind();
            }
        }

        private void GenerateData()
        {
            DataRow dtRow;

            _DataSet = new DataSet();
            _Dt.Columns.Add("種別", Type.GetType("System.String"));
            _Dt.Columns.Add("負荷[%]", Type.GetType("System.Double"));
            _DataSet.Tables.Add(_Dt);

            // データの追加
            dtRow = _DataSet.Tables[0].NewRow();
            dtRow[0] = "最大負荷";
            _DataSet.Tables[0].Rows.Add(dtRow);
            dtRow = _DataSet.Tables[0].NewRow();
            dtRow[0] = "直近負荷";
            _DataSet.Tables[0].Rows.Add(dtRow);
        }

        private void SetChart()
        {
            chart_Bar.DataSource = _DataSet;
            for (Int32 i = 1; i < _DataSet.Tables[0].Columns.Count; i++)
            {
                string columnName = _DataSet.Tables[0].Columns[i].ColumnName;

                chart_Bar.Series.Add(columnName);
                chart_Bar.Series[columnName].ChartType = SeriesChartType.Bar;
                chart_Bar.Series[columnName].XValueMember = _DataSet.Tables[0].Columns[0].ColumnName;
                chart_Bar.Series[columnName].YValueMembers = columnName;
            }
            chart_Bar.DataBind();
        }
    }
}
