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

        public MoveElementCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OneElement selected = Database.Instance.SelectedElement;
            List<OneElement> children = selected.Parent.Children;
            int curIdx = children.IndexOf(selected);

            switch ((string)parameter)
            {
                case "Up":
                    if (curIdx > 0)
                    {
                        children.Remove(selected);
                        children.Insert(curIdx - 1, selected);
                        Database.Instance.SelectedElement = selected;
                    }
                    break;
                case "Down":
                    if (curIdx < (children.Count - 1))
                    {
                        children.Remove(selected);
                        children.Insert(curIdx + 1, selected);
                        Database.Instance.SelectedElement = selected;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
