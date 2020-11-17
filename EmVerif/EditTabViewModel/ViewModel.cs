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
using EmVerif.Model;

namespace EmVerif.EditTabViewModel
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OneElement> TreeViewList
        {
            get
            {
                return Database.Instance.TreeViewList;
            }
        }
        public SelectedViewModel SelectedViewModel { get; private set; }
        public OneElement SelectedElement
        {
            get
            {
                return Database.Instance.SelectedElement;
            }
            set
            {
                Database.Instance.SelectedElement = value;
                SelectedViewModel = new SelectedViewModel(value);
                OnPropertyChanged("SelectedViewModel");
                OnPropertyChanged("SelectedElement");
            }
        }

        public StartStopButton StartStopButtonInstance { get; private set; }
        public AddElementCommand AddElementContextMenu { get; private set; }
        public DelElementCommand DelElementContextMenu { get; private set; }
        public MoveElementCommand MoveElementContextMenu { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            StartStopButtonInstance = new StartStopButton();
            AddElementContextMenu = new AddElementCommand();
            DelElementContextMenu = new DelElementCommand(this);
            MoveElementContextMenu = new MoveElementCommand();
        }

        public void Update()
        {
            OnPropertyChanged("TreeViewList");
            OnPropertyChanged("SelectedViewModel");
            OnPropertyChanged("SelectedElement");
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
