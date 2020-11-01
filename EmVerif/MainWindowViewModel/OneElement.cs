using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit.Document;

namespace EmVerif.MainWindowViewModel
{
    public class OneElement : INotifyPropertyChanged, IXmlSerializable
    {
        public class XmlIO
        {
            public ObservableCollection<OneElement> Children { get; set; }
            public bool IsExpanded { get; set; }
            public bool IsParentInclude { get; set; }
            public string Title { get; set; }
            public string Explanation { get; set; }
            public string ScriptContent { get; set; }
        }

        public ObservableCollection<OneElement> Children { get; private set; } = new ObservableCollection<OneElement>();
        public OneElement Parent { get; private set; } = null;
        public bool IsExpanded { get; set; } = true;
        private bool _IsParentInclude = true;
        public bool IsParentInclude
        {
            get
            {
                if (Parent == null)
                {
                    return false;
                }
                else
                {
                    return _IsParentInclude;
                }
            }
            set
            {
                _IsParentInclude = value;
                OnPropertyChanged("IncludedScriptDocument");
                OnPropertyChanged("IsParentInclude");
            }
        }
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
        private TextDocument _IncludedScriptDocument = new TextDocument();
        public TextDocument IncludedScriptDocument
        {
            get
            {
                if (IsParentInclude)
                {
                    if (Parent.IncludedScriptDocument.Text == "")
                    {
                        _IncludedScriptDocument.Text = Parent.ScriptDocument.Text;
                    }
                    else
                    {
                        _IncludedScriptDocument.Text = Parent.IncludedScriptDocument.Text + "\n" + Parent.ScriptDocument.Text;
                    }
                }
                else
                {
                    _IncludedScriptDocument.Text = "";
                }
                return _IncludedScriptDocument;
            }
            set
            {
                _IncludedScriptDocument = value;
                OnPropertyChanged("IncludedScriptDocument");
            }
        }
        private TextDocument _ScriptDocument = new TextDocument();
        public TextDocument ScriptDocument
        {
            get
            {
                return _ScriptDocument;
            }
            set
            {
                _ScriptDocument = value;
                OnPropertyChanged("ScriptDocument");
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
                    IsParentInclude = xmlIO.IsParentInclude;
                    Title = xmlIO.Title;
                    Explanation = xmlIO.Explanation.TrimStart();
                    _ScriptDocument.Text = xmlIO.ScriptContent.TrimStart();
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
            xmlIO.IsParentInclude = IsParentInclude;
            xmlIO.Title = Title;
            xmlIO.Explanation = "\r\n" + Explanation;
            xmlIO.ScriptContent = "\r\n" + _ScriptDocument.Text;

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

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
