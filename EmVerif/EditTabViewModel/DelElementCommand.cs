using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.EditTabViewModel
{
    class DelElementCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public DelElementCommand(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_RefViewModel.SelectedViewModel.Parent != null)
            {
                Database.Instance.SelectedElement.Parent.Children.Remove(Database.Instance.SelectedElement);
                _RefViewModel.SelectedViewModel.Parent.Children.Remove(_RefViewModel.SelectedViewModel);
            }
        }
    }
}
