using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.EditTabViewModel
{
    class AddElementCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public AddElementCommand(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_RefViewModel.SelectedViewModel != null)
            {
                OneElement oneElement = new OneElement(Database.Instance.SelectedElement);
                SelectedViewModel selectedViewModel = new SelectedViewModel(_RefViewModel.SelectedViewModel, oneElement);

                Database.Instance.SelectedElement.Children.Add(oneElement);
                _RefViewModel.SelectedViewModel.Children.Add(selectedViewModel);
            }
        }
    }
}
