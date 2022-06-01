using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.ExecTabViewModel
{
    class ExecTypeAddButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public ExecTypeAddButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if ((_RefViewModel.AddingExecType != "") && (!Database.Instance.ExecTypeList.Contains(_RefViewModel.AddingExecType)))
            {
                Database.Instance.ExecTypeList.Add(_RefViewModel.AddingExecType);
                SetDefaultExecFlag(Database.Instance.TreeViewList[0].Children);
                _RefViewModel.AddingExecType = "";
            }
        }

        private void SetDefaultExecFlag(IReadOnlyList<OneElement> oneElementList)
        {
            foreach (var oneElement in oneElementList)
            {
                if (oneElement.Children.Count != 0)
                {
                    SetDefaultExecFlag(oneElement.Children);
                }
                oneElement.ExecFlagDict[_RefViewModel.AddingExecType] = false;
            }
        }
    }
}
