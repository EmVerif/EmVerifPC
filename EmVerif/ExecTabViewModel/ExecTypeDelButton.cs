using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.ExecTabViewModel
{
    class ExecTypeDelButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public ExecTypeDelButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if ((_RefViewModel.SelectedExecType != "") && (Database.Instance.ExecTypeList.Contains(_RefViewModel.SelectedExecType)))
            {
                var dr = MessageBox.Show(_RefViewModel.SelectedExecType + "列を削除しますか？", "確認", MessageBoxButton.OKCancel);

                if (dr == MessageBoxResult.OK)
                {
                    Database.Instance.ExecTypeList.Remove(_RefViewModel.SelectedExecType);
                    DelExecFlag(Database.Instance.TreeViewList[0].Children);
                }
            }
        }

        private void DelExecFlag(IReadOnlyList<OneElement> oneElementList)
        {
            foreach (var oneElement in oneElementList)
            {
                if (oneElement.Children.Count != 0)
                {
                    DelExecFlag(oneElement.Children);
                }
                oneElement.ExecFlagDict.Remove(_RefViewModel.SelectedExecType);
            }
        }
    }
}
