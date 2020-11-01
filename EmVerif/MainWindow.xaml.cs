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

namespace EmVerif
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem == null)
            {
                return;
            }
            _ViewModel.SelectedElement = (OneElement)((TreeView)sender).SelectedItem;
        }
    }
}
