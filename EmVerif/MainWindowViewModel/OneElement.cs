using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EmVerif.MyControl;

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
        public MyString Title { get; set; } = new MyString();
        public MyString Explanation { get; set; } = new MyString();
        public MyString ScriptContent { get; set; } = new MyString();
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
            Title.Content = "トップ";
        }

        public OneElement(OneElement inParent)
        {
            Parent = inParent;
            Title.Content = "新規";
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
                    Title.Content = xmlIO.Title;
                    Explanation.Content = xmlIO.Explanation.TrimStart();
                    ScriptContent.Content = xmlIO.ScriptContent.TrimStart();
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
            xmlIO.Title = Title.Content;
            xmlIO.Explanation = "\r\n" + Explanation.Content;
            xmlIO.ScriptContent = "\r\n" + ScriptContent.Content;

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
