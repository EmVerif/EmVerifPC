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
    class CheckButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;
        private Boolean _CheckValue = false;

        public CheckButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if ((string)parameter == "Check")
            {
                _CheckValue = true;
            }
            else
            {
                _CheckValue = false;
            }
            if ((_RefViewModel.SelectedExecType != "") && (_RefViewModel.SelectedExecType != null))
            {
                if (_RefViewModel.Keyword == "")
                {
                    ConfirmCheck(_RefViewModel.SelectedExecType + "列を全て" + _CheckValue + "に変更しますか？");
                }
                else
                {
                    ConfirmCheck(_RefViewModel.SelectedExecType + "列で、「" + _RefViewModel.Keyword + "」文字列をタイトルに含む行を全て" + _CheckValue + "に変更しますか？");
                }
            }
        }

        private void ConfirmCheck(string inMessage)
        {
            var dr = MessageBox.Show(inMessage, "確認", MessageBoxButton.OKCancel);

            if (dr == MessageBoxResult.OK)
            {
                CheckIfContainsKeyword();
                _RefViewModel.Update();
            }
        }

        private void CheckIfContainsKeyword()
        {
            var results = _RefViewModel.ChainTitleList.Zip(_RefViewModel.OneElementDependTitleList, (chainTitle, oneElement) => new { ChainTitle = chainTitle, OneElement = oneElement });
            foreach (var result in results)
            {
                if (result.ChainTitle.Contains(_RefViewModel.Keyword))
                {
                    result.OneElement.ExecFlagDict[_RefViewModel.SelectedExecType] = _CheckValue;
                }
            }
        }
    }
}
