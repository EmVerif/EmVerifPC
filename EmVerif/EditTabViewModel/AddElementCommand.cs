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

        public AddElementCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (Database.Instance.SelectedElement != null)
            {
                OneElement oneElement = new OneElement(Database.Instance.SelectedElement);
                Database.Instance.SelectedElement.Children.Add(oneElement);
            }
        }
    }
}
