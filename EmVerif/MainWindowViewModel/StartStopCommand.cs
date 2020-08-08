using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using EmVerif.Core.Script;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmVerif.MainWindowViewModel
{
    class StartStopCommand : ICommand, INotifyPropertyChanged
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
                OnPropertyChanged("DisplayString");
            }
        }
        private string _DisplayString;

        private MainWindowViewModel _RefViewModel;
        private const string _StartStr = "開始";
        private const string _StopStr = "停止";

        public StartStopCommand(MainWindowViewModel vm)
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
            if (DisplayString == _StopStr)
            {
                StopScript(this, new EventArgs());
            }
            else
            {
                StartScript(_RefViewModel.Script.Text, _RefViewModel.SelectedIpAddress);
            }
        }

        public void ForceEnd()
        {
            if (DisplayString == _StopStr)
            {
                StopScript(this, new EventArgs());
            }
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        async private void StartScript(string inRoslynText, IPAddress inIpAddress)
        {
            try
            {
                ScriptOptions options = ScriptOptions.Default.WithImports("System", "System.Collections.Generic");
                PublicController.Instance.Reset();
                Script<object> script = CSharpScript.Create(inRoslynText, options, typeof(PublicApis));
                await script.RunAsync(new PublicApis());
                PublicController.Instance.EndEvent += StopScript;
                PublicController.Instance.StartScript(inIpAddress);
                DisplayString = _StopStr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopScript(object sender, EventArgs e)
        {
            PublicController.Instance.StopScript();
            DisplayString = _StartStr;
        }
    }
}
