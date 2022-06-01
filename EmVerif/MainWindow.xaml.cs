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
            _EditTab.ViewModel.Update();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _EditTab.ViewModel.CloseWindow();
            _ExecTab.ViewModel.CloseWindow();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
            {
                if (e.AddedItems[0].GetType() == typeof(TabItem))
                {
                    switch (((TabControl)sender).SelectedIndex)
                    {
                        case 0:
                            _EditTab.ViewModel.Update();
                            break;
                        case 1:
                            break;
                    }
                }
            }
        }
    }
}
