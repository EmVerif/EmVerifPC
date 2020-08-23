using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmVerif.MainWindowViewModel
{
    class DelElementCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public DelElementCommand(ViewModel vm)
        {
            _RefViewModel = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        public bool CanExecute(object parameter)
        {
            bool ret;

            if (_RefViewModel.SelectedElement.Parent == null)
            {
                ret = false;
            }
            else
            {
                ret = true;
            }

            return ret;
        }

        public void Execute(object parameter)
        {
            _RefViewModel.SelectedElement.Parent.Children.Remove(_RefViewModel.SelectedElement);
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedElement")
            {
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }
    }
}
