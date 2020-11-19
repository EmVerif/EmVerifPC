using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmVerif.ExecTabViewModel
{
    class ExecButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public ExecButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
