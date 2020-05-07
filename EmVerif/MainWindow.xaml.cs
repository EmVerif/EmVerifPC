using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmVerif.Script;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RoslynPad.Editor;
using RoslynPad.Roslyn;

namespace EmVerif
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private CustomRoslynHost _Host;

        public MainWindow()
        {
            InitializeComponent();
            _Host = new CustomRoslynHost(
                additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows")
                },
                references: RoslynHostReferences.Default.With(typeNamespaceImports: new[] { typeof(PublicApis) })
            );
            DataContext = MainWindowDataContext.Instance;
            comboBox.SelectedIndex = 0;
        }

        private void RoslynCodeEditor_Loaded(object sender, RoutedEventArgs e)
        {
            roslynCodeEditor.Initialize(_Host, new ClassificationHighlightColors(), Directory.GetCurrentDirectory(), String.Empty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            MainWindowDataContext.Instance.ClickStartButton(roslynCodeEditor.Text, (IPAddress)comboBox.SelectedItem);
            button.IsEnabled = true;
        }
    }

    class MainWindowDataContext : INotifyPropertyChanged
    {
        public static MainWindowDataContext Instance = new MainWindowDataContext();
        public string ButtonText
        {
            get
            {
                return _ButtonText;
            }
            set
            {
                _ButtonText = value;
                OnPropertyChanged("ButtonText");
            }
        }
        public IEnumerable<IPAddress> ItemsSource { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private const string _StartStr = "開始";
        private const string _StopStr = "停止";

        private string _ButtonText;

        public MainWindowDataContext()
        {
            ButtonText = _StartStr;
            ItemsSource = PublicController.Instance.GetIpV4List(); 
        }

        public void ClickStartButton(string inRoslynText, IPAddress inIpAddress)
        {
            if (ButtonText == _StopStr)
            {
                StopScript(this, new EventArgs());
            }
            else
            {
                StartScript(inRoslynText, inIpAddress);
            }
        }

        protected void OnPropertyChanged(string info)
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
                MainWindowDataContext.Instance.ButtonText = _StopStr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopScript(object sender, EventArgs e)
        {
            PublicController.Instance.StopScript();
            MainWindowDataContext.Instance.ButtonText = _StartStr;
        }
    }
}
