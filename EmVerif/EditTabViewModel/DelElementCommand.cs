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

        public DelElementCommand(ViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        public bool CanExecute(object parameter)
        {
            bool ret;

            if (Database.Instance.SelectedElement.Parent == null)
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
            Database.Instance.SelectedElement.Parent.Children.Remove(Database.Instance.SelectedElement);
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
