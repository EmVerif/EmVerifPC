using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using EmVerif.Core.Script;
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
            roslynCodeEditor.Initialize(_Host, new ClassificationHighlightColors(), Directory.GetCurrentDirectory(), String.Empty);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ViewModel.CloseWindow();
        }
    }
}
