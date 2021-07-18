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
using EmVerif.Model;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmVerif.EditTabViewModel
{
    class StartStopButton : ICommand, INotifyPropertyChanged
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

        private const string _StartStr = "開始";
        private const string _StopStr = "停止";

        public StartStopButton()
        {
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
                string workFolder = "";
                OneElement oneElement = Database.Instance.SelectedElement;

                while (oneElement.Parent != null)
                {
                    workFolder = @"\" + oneElement.Title + workFolder;
                    oneElement = oneElement.Parent;
                }
                workFolder = @".\" + oneElement.Title + workFolder;
                StartScript(
                    Database.Instance.SelectedElement.IncludedScriptDocument.Text + "\n" + Database.Instance.SelectedElement.ScriptDocument.Text,
                    Database.Instance.SelectedIpAddress,
                    workFolder
                );
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

        private void StartScript(string inRoslynText, IPAddress inIpAddress, string inWorkFolder)
        {
            try
            {
                ScriptOptions options = ScriptOptions.Default.WithImports("System", "System.Collections.Generic");
                PublicController.Instance.Reset(inWorkFolder);
                Script<object> script = CSharpScript.Create(inRoslynText, options, typeof(PublicApis));
                script.RunAsync(new PublicApis()).Wait();
                PublicController.Instance.EndEvent += StopScript;
                PublicController.Instance.StartScript(inIpAddress);
                DisplayString = _StopStr;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void StopScript(object sender, EventArgs e)
        {
            PublicController.Instance.StopScript();
            DisplayString = _StartStr;
        }
    }
}
