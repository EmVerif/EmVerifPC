using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using EmVerif.Core.Script;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmVerif.MainWindowViewModel
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OneElement> TreeViewList { get; set; } = new ObservableCollection<OneElement>();
        private OneElement _SelectedElement = new OneElement();
        public OneElement SelectedElement
        {
            get
            {
                return _SelectedElement;
            }
            set
            {
                _SelectedElement = value;
                OnPropertyChanged("SelectedElement");
            }
        }
        public IEnumerable<IPAddress> IpAddressList { get; private set; }
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
                OnPropertyChanged("SelectedIpAddress");
            }
        }
        public StartStopButton StartStopButtonInstance { get; private set; }
        public SaveButton SaveButtonInstance { get; private set; }
        public LoadButton LoadButtonInstance { get; private set; }
        public AddElementCommand AddElementContextMenu { get; private set; }
        public DelElementCommand DelElementContextMenu { get; private set; }
        public MoveElementCommand MoveElementContextMenu { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            IpAddressList = PublicController.Instance.GetIpV4List();
            StartStopButtonInstance = new StartStopButton(this);
            SaveButtonInstance = new SaveButton(this);
            LoadButtonInstance = new LoadButton(this);
            AddElementContextMenu = new AddElementCommand(this);
            DelElementContextMenu = new DelElementCommand(this);
            MoveElementContextMenu = new MoveElementCommand(this);
            TreeViewList.Add(SelectedElement);
        }

        public void CloseWindow()
        {
            StartStopButtonInstance.ForceEnd();
        }

        public void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
