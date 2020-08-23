using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmVerif.MainWindowViewModel
{
    class MoveElementCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public MoveElementCommand(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OneElement selected = _RefViewModel.SelectedElement;
            ObservableCollection<OneElement> children = selected.Parent.Children;
            int curIdx = children.IndexOf(_RefViewModel.SelectedElement);

            switch ((string)parameter)
            {
                case "Up":
                    if (curIdx > 0)
                    {
                        children.Remove(selected);
                        children.Insert(curIdx - 1, selected);
                        _RefViewModel.SelectedElement = selected;
                    }
                    break;
                case "Down":
                    if (curIdx < (_RefViewModel.SelectedElement.Parent.Children.Count - 1))
                    {
                        children.Remove(selected);
                        children.Insert(curIdx + 1, selected);
                        _RefViewModel.SelectedElement = selected;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
