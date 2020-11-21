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
        public ObservableCollection<SelectedViewModel> TreeViewList { get; private set; }

        private SelectedViewModel _SelectedViewModel;
        public SelectedViewModel SelectedViewModel
        {
            get
            {
                return _SelectedViewModel;
            }
            set
            {
                Database.Instance.SelectedElement = value.RefModel;
                _SelectedViewModel = value;
                OnPropertyChanged("SelectedViewModel");
            }
        }

        public StartStopButton StartStopButtonInstance { get; private set; }
        public AddElementCommand AddElementContextMenu { get; private set; }
        public DelElementCommand DelElementContextMenu { get; private set; }
        public MoveElementCommand MoveElementContextMenu { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            TreeViewList = new ObservableCollection<SelectedViewModel>();
            TreeViewList.Add(new SelectedViewModel(null, Database.Instance.SelectedElement));
            StartStopButtonInstance = new StartStopButton();
            AddElementContextMenu = new AddElementCommand(this);
            DelElementContextMenu = new DelElementCommand(this);
            MoveElementContextMenu = new MoveElementCommand(this);
        }

        public void Update()
        {
            TreeViewList = new ObservableCollection<SelectedViewModel>();
            TreeViewList.Add(new SelectedViewModel(null, Database.Instance.SelectedElement));
            MakeTableViewFromDatabase(Database.Instance.TreeViewList[0].Children, TreeViewList[0]);
            OnPropertyChanged("TreeViewList");
            OnPropertyChanged("SelectedViewModel");
        }

        public void CloseWindow()
        {
            StartStopButtonInstance.ForceEnd();
        }

        public void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void MakeTableViewFromDatabase(IReadOnlyList<OneElement> oneElementList, SelectedViewModel parentSelectedViewModel)
        {
            foreach (var oneElement in oneElementList)
            {
                SelectedViewModel selectedViewModel = new SelectedViewModel(parentSelectedViewModel, oneElement);

                parentSelectedViewModel.Children.Add(selectedViewModel);
                if (oneElement.Children.Count != 0)
                {
                    MakeTableViewFromDatabase(oneElement.Children, selectedViewModel);
                }
            }
        }
    }
}
