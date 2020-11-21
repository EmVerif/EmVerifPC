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
        public ObservableCollection<SelectedViewModel> Children { get; private set; } = new ObservableCollection<SelectedViewModel>();
        public bool IsExpanded
        {
            get
            {
                return RefModel.IsExpanded;
            }
            set
            {
                RefModel.IsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        public bool IsParentInclude
        {
            get
            {
                return RefModel.IsParentInclude;
            }
            set
            {
                RefModel.IsParentInclude = value;
                OnPropertyChanged("IsParentInclude");
            }
        }
        public string Title
        {
            get
            {
                return RefModel.Title;
            }
            set
            {
                RefModel.Title = value;
                OnPropertyChanged("Title");
            }
        }
        public string Explanation
        {
            get
            {
                return RefModel.Explanation;
            }
            set
            {
                RefModel.Explanation = value;
                OnPropertyChanged("Explanation");
            }
        }
        public TextDocument IncludedScriptDocument
        {
            get
            {
                return RefModel.IncludedScriptDocument;
            }
        }
        public TextDocument ScriptDocument
        {
            get
            {
                return RefModel.ScriptDocument;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                bool ret;

                if (RefModel.Parent == null)
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

        public SelectedViewModel Parent;
        public OneElement RefModel { get; private set; }

        public SelectedViewModel(SelectedViewModel selectedViewModel, OneElement oneElement)
        {
            Parent = selectedViewModel;
            RefModel = oneElement;
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
