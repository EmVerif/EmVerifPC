using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using EmVerif.Model;

using ICSharpCode.AvalonEdit.Document;

namespace EmVerif.EditTabViewModel
{
    public class SelectedViewModel : INotifyPropertyChanged
    {
        public bool IsExpanded
        {
            get
            {
                return _RefModel.IsExpanded;
            }
            set
            {
                _RefModel.IsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        public bool IsParentInclude
        {
            get
            {
                return _RefModel.IsParentInclude;
            }
            set
            {
                _RefModel.IsParentInclude = value;
                OnPropertyChanged("IsParentInclude");
            }
        }
        public string Title
        {
            get
            {
                return _RefModel.Title;
            }
            set
            {
                _RefModel.Title = value;
                OnPropertyChanged("Title");
            }
        }
        public string Explanation
        {
            get
            {
                return _RefModel.Explanation;
            }
            set
            {
                _RefModel.Explanation = value;
                OnPropertyChanged("Explanation");
            }
        }
        public TextDocument IncludedScriptDocument
        {
            get
            {
                return _RefModel.IncludedScriptDocument;
            }
        }
        public TextDocument ScriptDocument
        {
            get
            {
                return _RefModel.ScriptDocument;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                bool ret;

                if (_RefModel.Parent == null)
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
        private OneElement _RefModel;

        public SelectedViewModel(OneElement oneElement)
        {
            _RefModel = oneElement;
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
