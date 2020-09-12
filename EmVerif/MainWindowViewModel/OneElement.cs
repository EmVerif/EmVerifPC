using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace EmVerif.MainWindowViewModel
{
    public class OneElement : INotifyPropertyChanged, IXmlSerializable
    {
        public class XmlIO
        {
            public ObservableCollection<OneElement> Children { get; set; }
            public bool IsExpanded { get; set; }
            public string Title { get; set; }
            public string Explanation { get; set; }
            public string ScriptContent { get; set; }
        }

        public ObservableCollection<OneElement> Children { get; private set; } = new ObservableCollection<OneElement>();
        public OneElement Parent { get; private set; } = null;
        public bool IsExpanded { get; set; } = true;
        private string _Title = "";
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }
        private string _Explanation = "";
        public string Explanation
        {
            get
            {
                return _Explanation;
            }
            set
            {
                _Explanation = value;
                OnPropertyChanged("Explanation");
            }
        }
        private string _ScriptContent = "";
        public string Script
        {
            get
            {
                return _ScriptContent;
            }
            set
            {
                _ScriptContent = value;
                OnPropertyChanged("ScriptContent");
            }
        }
        public bool IsReadOnly
        {
            get
            {
                bool ret;

                if (Parent == null)
                {
                    ret = true;
                }
                else
                {
                    ret = false;
                }

                return ret;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OneElement()
        {
            Title = "トップ";
        }

        public OneElement(OneElement inParent)
        {
            Parent = inParent;
            Title = "新規";
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlIO));

            reader.Read();
            if (reader.IsEmptyElement)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                XmlIO xmlIO = serializer.Deserialize(reader) as XmlIO;

                if (xmlIO != null)
                {
                    Children = xmlIO.Children;
                    IsExpanded = xmlIO.IsExpanded;
                    Title = xmlIO.Title;
                    Explanation = xmlIO.Explanation.TrimStart();
                    Script = xmlIO.ScriptContent.TrimStart();
                }
            }
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlIO xmlIO = new XmlIO();
            XmlSerializer serializer = new XmlSerializer(typeof(XmlIO));

            xmlIO.Children = Children;
            xmlIO.IsExpanded = IsExpanded;
            xmlIO.Title = Title;
            xmlIO.Explanation = "\r\n" + Explanation;
            xmlIO.ScriptContent = "\r\n" + Script;

            serializer.Serialize(writer, xmlIO);
        }

        public void SetParent(OneElement inParent)
        {
            Parent = inParent;
            foreach (var child in Children)
            {
                child.SetParent(this);
            }
        }
    }
}
