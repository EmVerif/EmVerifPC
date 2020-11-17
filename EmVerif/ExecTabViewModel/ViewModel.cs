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
        public DataView DataView
        {
            get
            {
                return new DataView(_DataTable);
            }
        }
        private DataTable _DataTable
        {
            get
            {
                var dt = new DataTable();
                List<string> chainTitleList = new List<string>();

                dt.Columns.Add("タイトル", typeof(string));
                dt.Columns[0].ReadOnly = true;

                GetTitleList(Database.Instance.TreeViewList[0].Children, new List<string>(), chainTitleList);
                foreach (var chainTitle in chainTitleList)
                {
                    dt.Rows.Add(new object[] { chainTitle });
                }

                return dt;
            }
        }

        public void Update()
        {
            OnPropertyChanged("DataView");
        }

        private void GetTitleList(IReadOnlyList<OneElement> oneElementList, IReadOnlyList<string> titleList, List<string> chainTitleList)
        {
            foreach (var oneElement in oneElementList)
            {
                var list = new List<string>(titleList);

                list.Add(oneElement.Title);
                if (oneElement.Children.Count != 0)
                {
                    GetTitleList(oneElement.Children, list, chainTitleList);
                }
                else
                {
                    string chainTitle = "";

                    foreach (var title in list)
                    {
                        chainTitle += title + "／";
                    }
                    chainTitleList.Add(chainTitle.Trim('／'));
                }
            }
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
