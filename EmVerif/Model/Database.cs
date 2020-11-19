using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EmVerif.Model
{
    public class Database
    {
        public class XmlIO
        {
            public List<string> ExecTypeList { get; set; }
            public ObservableCollection<OneElement> TreeViewList { get; set; }
        }
        public static Database Instance = new Database();
        public ObservableCollection<OneElement> TreeViewList { get; private set; } = new ObservableCollection<OneElement>();
        public OneElement SelectedElement { get; set; } = new OneElement();
        public IPAddress SelectedIpAddress { get; set; }
        public List<string> ExecTypeList { get; private set; } = new List<string>();

        public Database()
        {
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
                ExecTypeList = new List<string>();
                TreeViewList = new ObservableCollection<OneElement>();
                SelectedElement = new OneElement();
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
    }
}
