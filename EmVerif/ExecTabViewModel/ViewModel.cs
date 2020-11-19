using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using EmVerif.Model;

namespace EmVerif.ExecTabViewModel
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> ExecTypeList
        {
            get
            {
                var ret = new ObservableCollection<string>(Database.Instance.ExecTypeList);

                return ret;
            }
        }
        public string SelectedExecType { get; set; } = null;
        public ExecButton ExecButtonInstance { get; private set; }
        public ExecTypeDelButton ExecTypeDelButtonInstance { get; private set; }
        public string Keyword { get; set; } = "";
        public CheckButton CheckButtonInstance { get; private set; }
        public string AddingExecType { get; set; } = "";
        public ExecTypeAddButton ExecTypeAddButtonInstance { get; private set; }
        public DataView DataView
        {
            get
            {
                return new DataView(_DataTable);
            }
        }

        public List<string> ChainTitleList = new List<string>();
        public List<Dictionary<string, Boolean>> ExecFlagDictList = new List<Dictionary<string, bool>>();
        private DataTable _DataTable { get; set; } = new DataTable();

        public ViewModel()
        {
            ExecButtonInstance = new ExecButton(this);
            ExecTypeDelButtonInstance = new ExecTypeDelButton(this);
            ExecTypeAddButtonInstance = new ExecTypeAddButton(this);
            CheckButtonInstance = new CheckButton(this);
        }

        public void Update()
        {
            MakeDataTableFromDatabase();
            OnPropertyChanged("ExecTypeList");
            OnPropertyChanged("AddingExecType");
            OnPropertyChanged("DataView");
        }

        private void MakeDataTableFromDatabase()
        {
            _DataTable.RowChanged -= _DataTable_RowChanged;
            _DataTable.Clear();
            _DataTable.Columns.Clear();
            ChainTitleList = new List<string>();
            ExecFlagDictList = new List<Dictionary<string, bool>>();

            _DataTable.Columns.Add("タイトル", typeof(string));
            _DataTable.Columns[0].ReadOnly = true;
            foreach (var execType in Database.Instance.ExecTypeList)
            {
                _DataTable.Columns.Add(execType, typeof(Boolean));
            }

            GetTitleList(Database.Instance.TreeViewList[0].Children, new List<string>(), ChainTitleList, ExecFlagDictList);
            var results = ChainTitleList.Zip(ExecFlagDictList, (chainTitle, execFlagDict) => new { ChainTitle = chainTitle, ExecFlagDict = execFlagDict });
            foreach (var result in results)
            {
                List<object> row = new List<object>() { result.ChainTitle };

                foreach (var execType in Database.Instance.ExecTypeList)
                {
                    if (result.ExecFlagDict.ContainsKey(execType))
                    {
                        row.Add(result.ExecFlagDict[execType]);
                    }
                    else
                    {
                        row.Add(false);
                    }
                }
                _DataTable.Rows.Add(row.ToArray());
            }
            _DataTable.RowChanged += _DataTable_RowChanged;
        }

        private void _DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            int idx = _DataTable.Rows.IndexOf(e.Row);

            var cellList = e.Row.ItemArray.ToList();
            cellList.RemoveAt(0);
            var execInfoList = cellList.Zip(Database.Instance.ExecTypeList, (execFlag, execType) => new { ExecFlag = execFlag, ExecType = execType });
            foreach (var execInfo in execInfoList)
            {
                ExecFlagDictList[idx][execInfo.ExecType] = (Boolean)execInfo.ExecFlag;
            }
        }

        private void GetTitleList(IReadOnlyList<OneElement> oneElementList, IReadOnlyList<string> titleList, List<string> chainTitleList, List<Dictionary<string, bool>> execFlagDictList)
        {
            foreach (var oneElement in oneElementList)
            {
                var list = new List<string>(titleList);

                list.Add(oneElement.Title);
                if (oneElement.Children.Count != 0)
                {
                    GetTitleList(oneElement.Children, list, chainTitleList, execFlagDictList);
                }
                else
                {
                    string chainTitle = "";

                    foreach (var title in list)
                    {
                        chainTitle += title + "／";
                    }
                    chainTitleList.Add(chainTitle.Trim('／'));
                    execFlagDictList.Add(oneElement.ExecFlagDict);
                }
            }
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
