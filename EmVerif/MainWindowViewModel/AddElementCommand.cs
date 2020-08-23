using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmVerif.MainWindowViewModel
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
            if (_RefViewModel.SelectedElement != null)
            {
                OneElement oneElement = new OneElement(_RefViewModel.SelectedElement);
                _RefViewModel.SelectedElement.Children.Add(oneElement);
            }
        }
    }
}
