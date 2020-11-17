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
    class Database
    {
        public static Database Instance = new Database();
        public ObservableCollection<OneElement> TreeViewList { get; private set; } = new ObservableCollection<OneElement>();
        public OneElement SelectedElement { get; set; } = new OneElement();
        public IPAddress SelectedIpAddress { get; set; }

        public Database()
        {
            TreeViewList.Add(SelectedElement);
        }

        public void Save(string inFileName)
        {
            var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<OneElement>));

            using (var sw = new StreamWriter(inFileName, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(sw, TreeViewList);
                sw.Flush();
            }
        }

        public void Load(string inFileName)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<OneElement>));

                using (FileStream fs = new FileStream(inFileName, FileMode.Open))
                {
                    TreeViewList = xmlSerializer.Deserialize(fs) as ObservableCollection<OneElement>;
                    SelectedElement = TreeViewList[0];
                    SetParent();
                }
            }
            catch (Exception e)
            {
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
