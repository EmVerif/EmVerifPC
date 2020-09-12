using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmVerif.Core.Script;
using EmVerif.MainWindowViewModel;
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
        private bool _SelectedChangedLock = false;

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
            _ViewModel.CloseWindow();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        //_ViewModel.MoveElementContextMenu.Execute("Up");
                        break;
                    case Key.Down:
                        //_ViewModel.MoveElementContextMenu.Execute("Down");
                        break;
                    default:
                        break;
                }
            }
        }

        private void TreeView_KeyUp(object sender, KeyEventArgs e)
        {
            SetSelectedElement((TreeView)sender);
        }

        private void TreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetSelectedElement((TreeView)sender);
        }

        private async void SetSelectedElement(TreeView sender)
        {
            if ((_SelectedChangedLock) || (sender.SelectedItem == null))
            {
                return;
            }
            _SelectedChangedLock = true;
            await Task.Delay(300);
            _ViewModel.SelectedElement.Script = roslynCodeEditor.Text;
            _ViewModel.SelectedElement = (OneElement)sender.SelectedItem;
            roslynCodeEditor.Text = _ViewModel.SelectedElement.Script;
            _SelectedChangedLock = false;
        }
    }
}
