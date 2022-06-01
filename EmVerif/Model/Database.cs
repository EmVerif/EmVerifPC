using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EmVerif.Model
{
    public class Database : INotifyPropertyChanged
    {
        public class XmlIO
        {
            public ObservableCollection<string> ExecTypeList { get; set; }
            public ObservableCollection<OneElement> TreeViewList { get; set; }
        }
        public static Database Instance = new Database();

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<OneElement> _TreeViewList = new ObservableCollection<OneElement>();
        public ObservableCollection<OneElement> TreeViewList
        {
            get
            {
                return _TreeViewList;
            }
            private set
            {
                _TreeViewList = value;
                OnPropertyChanged(nameof(TreeViewList));
            }
        }

        private OneElement _SelectedElement;
        public OneElement SelectedElement
        {
            get
            {
                return _SelectedElement;
            }
            set
            {
                _SelectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        private IPAddress _SelectedIpAddress;
        public IPAddress SelectedIpAddress
        {
            get
            {
                return _SelectedIpAddress;
            }
            set
            {
                _SelectedIpAddress = value;
                OnPropertyChanged(nameof(SelectedIpAddress));
            }
        }

        private ObservableCollection<string> _ExecTypeList = new ObservableCollection<string>();
        public ObservableCollection<string> ExecTypeList
        {
            get
            {
                return _ExecTypeList;
            }
            private set
            {
                _ExecTypeList = value;
                OnPropertyChanged(nameof(ExecTypeList));
            }
        }

        public Database()
        {
            SelectedElement = new OneElement(ExecTypeList);
            TreeViewList.Add(SelectedElement);
        }

        public void Save(string inFileName)
        {
            var xmlSerializer = new XmlSerializer(typeof(XmlIO));

            using (var sw = new StreamWriter(inFileName, false, Encoding.UTF8))
            {
                var xmlIO = new XmlIO();

                xmlIO.ExecTypeList = ExecTypeList;
                xmlIO.TreeViewList = TreeViewList;
                xmlSerializer.Serialize(sw, xmlIO);
                sw.Flush();
            }
        }

        public void Load(string inFileName)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(XmlIO));

                using (FileStream fs = new FileStream(inFileName, FileMode.Open))
                {
                    var xmlIO = xmlSerializer.Deserialize(fs) as XmlIO;

                    ExecTypeList = xmlIO.ExecTypeList;
                    TreeViewList = xmlIO.TreeViewList;
                    SelectedElement = TreeViewList[0];
                    SetParent();
                }
            }
            catch (Exception e)
            {
                ExecTypeList = new ObservableCollection<string>();
                TreeViewList = new ObservableCollection<OneElement>();
                SelectedElement = new OneElement(Database.Instance.ExecTypeList);
                TreeViewList.Add(SelectedElement);
                throw e;
            }
        }

        private void SetParent()
        {
            foreach (var treeView in Database.Instance.TreeViewList)
            {
                treeView.SetParent(null);
            }
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
