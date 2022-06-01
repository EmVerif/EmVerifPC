using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.EditTabViewModel
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
            SelectedViewModel selectedViewModel = _RefViewModel.SelectedViewModel;
            ObservableCollection<SelectedViewModel> childrenSelectedViewModel = selectedViewModel.Parent.Children;
            OneElement oneElement = Database.Instance.SelectedElement;
            ObservableCollection<OneElement> childrenOneElement = oneElement.Parent.Children;
            int curIdx = childrenSelectedViewModel.IndexOf(selectedViewModel);

            switch ((string)parameter)
            {
                case "Up":
                    if (curIdx > 0)
                    {
                        childrenSelectedViewModel.Remove(selectedViewModel);
                        childrenSelectedViewModel.Insert(curIdx - 1, selectedViewModel);
                        childrenOneElement.Remove(oneElement);
                        childrenOneElement.Insert(curIdx - 1, oneElement);
                    }
                    break;
                case "Down":
                    if (curIdx < (childrenSelectedViewModel.Count - 1))
                    {
                        childrenSelectedViewModel.Remove(selectedViewModel);
                        childrenSelectedViewModel.Insert(curIdx + 1, selectedViewModel);
                        childrenOneElement.Remove(oneElement);
                        childrenOneElement.Insert(curIdx + 1, oneElement);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
