using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using EmVerif.Core.Script;
using EmVerif.Model;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmVerif.ExecTabViewModel
{
    class ExecButton : ICommand, INotifyPropertyChanged
    {
        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public string DisplayString
        {
            get
            {
                return _DisplayString;
            }
            private set
            {
                _DisplayString = value;
                OnPropertyChanged(nameof(DisplayString));
            }
        }
        private string _DisplayString;

        private const string _StartStr = "選択列実行";
        private const string _StopStr = "停止";

        private ViewModel _RefViewModel;
        private bool _FinFlag;

        public ExecButton(ViewModel vm)
        {
            _RefViewModel = vm;
            DisplayString = _StartStr;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if ((_RefViewModel.SelectedExecType == null) || (_RefViewModel.SelectedExecType == ""))
            {
                MessageBox.Show("列を選択してください。");
            }
            else if (DisplayString == _StopStr)
            {
                StopScript(this, new EventArgs());
                DisplayString = _StartStr;
            }
            else
            {
                var dr = MessageBox.Show(_RefViewModel.SelectedExecType + "列を連続実行しますか？", "確認", MessageBoxButton.OKCancel);

                if (dr == MessageBoxResult.OK)
                {
                    ExecCore();
                }
            }
        }

        async void ExecCore()
        {
            DisplayString = _StopStr;
            var execList = _RefViewModel.OneElementDependTitleList.Where((x) => x.ExecFlagDict[_RefViewModel.SelectedExecType] == true);
            foreach (var execOneElement in execList)
            {
                string workFolder = "";
                var oneElement = execOneElement;

                while (oneElement.Parent != null)
                {
                    workFolder = @"\" + oneElement.Title + workFolder;
                    oneElement = oneElement.Parent;
                }
                workFolder = @".\" + _RefViewModel.SelectedExecType + @"\" + oneElement.Title + workFolder;

                _FinFlag = false;
                try
                {
                    StartScript(
                        execOneElement.IncludedScriptDocument.Text + "\n" + execOneElement.ScriptDocument.Text,
                        Database.Instance.SelectedIpAddress,
                        workFolder
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
                while (!_FinFlag)
                {
                    await Task.Delay(100);
                }

                if (DisplayString == _StartStr)
                {
                    break;
                }
            }
            DisplayString = _StartStr;
        }

        public void ForceEnd()
        {
            if (DisplayString == _StopStr)
            {
                StopScript(this, new EventArgs());
                DisplayString = _StartStr;
            }
        }

        private void StartScript(string inRoslynText, IPAddress inIpAddress, string inWorkFolder)
        {
            ScriptOptions options = ScriptOptions.Default.WithImports("System", "System.Collections.Generic");
            PublicController.Instance.Reset(inWorkFolder);
            Script<object> script = CSharpScript.Create(inRoslynText, options, typeof(PublicApis));
            script.RunAsync(new PublicApis()).Wait();
            PublicController.Instance.EndEvent += StopScript;
            PublicController.Instance.StartScript(inIpAddress);
        }

        private void StopScript(object sender, EventArgs e)
        {
            PublicController.Instance.StopScript();
            _FinFlag = true;
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
