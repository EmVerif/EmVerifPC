using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmVerif.Core.Script;
using EmVerif.EditTabViewModel;
using EmVerif.Model;

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
            ViewModel.LoadButtonInstance.LoadFinished += LoadButtonInstance_LoadFinished;
        }

        private void LoadButtonInstance_LoadFinished(object sender, EventArgs e)
        {
            Update();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _EditTab.ViewModel.CloseWindow();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            _ExecTab.ViewModel.Update();
            _EditTab.ViewModel.Update();
        }
    }
}
