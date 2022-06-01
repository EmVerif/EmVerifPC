using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using EmVerif.Common;

using ICSharpCode.AvalonEdit.Document;

namespace EmVerif.Model
{
    public class OneElement : IXmlSerializable
    {
        public class XmlIO
        {
            public ObservableCollection<OneElement> Children { get; set; }
            public bool IsExpanded { get; set; }
            public bool IsParentInclude { get; set; }
            public string Title { get; set; }
            public string Explanation { get; set; }
            public string ScriptContent { get; set; }
            public SerializableDictionary<string, Boolean> ExecFlagDict { get; set; }
        }
        public OneElement Parent { get; private set; } = null;
        public ObservableCollection<OneElement> Children { get; private set; } = new ObservableCollection<OneElement>();
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
            }
        }
        public string Title { get; set; } = "";
        public string Explanation { get; set; } = "";
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
            private set
            {
                _IncludedScriptDocument = value;
            }
        }
        public TextDocument ScriptDocument { get; private set; } = new TextDocument();
        public SerializableDictionary<string, Boolean> ExecFlagDict { get; private set; } = new SerializableDictionary<string, bool>();

        public OneElement()
        {
            Title = "トップ";
        }

        public OneElement(IReadOnlyList<string> inExecTypeList)
        {
            Title = "トップ";
            foreach (var execType in inExecTypeList)
            {
                ExecFlagDict.Add(execType, false);
            }
        }

        public OneElement(OneElement inParent, IReadOnlyList<string> inExecTypeList)
        {
            Parent = inParent;
            Title = "新規";
            foreach (var execType in inExecTypeList)
            {
                ExecFlagDict.Add(execType, false);
            }
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
                    ScriptDocument.Text = xmlIO.ScriptContent.TrimStart();
                    ExecFlagDict = xmlIO.ExecFlagDict;
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
            xmlIO.ScriptContent = "\r\n" + ScriptDocument.Text;
            xmlIO.ExecFlagDict = ExecFlagDict;

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
