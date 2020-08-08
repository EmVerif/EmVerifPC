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
using ICSharpCode.AvalonEdit.Document;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmVerif.MainWindowViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public IEnumerable<IPAddress> IpAddressList { get; private set; }
        public IPAddress SelectedIpAddress { get; set; }
        public TextDocument Script { get; set; } = new TextDocument();
        public StartStopCommand ButtonClickCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            IpAddressList = PublicController.Instance.GetIpV4List();
            ButtonClickCommand = new StartStopCommand(this);
        }

        public void CloseWindow()
        {
            ButtonClickCommand.ForceEnd();
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
