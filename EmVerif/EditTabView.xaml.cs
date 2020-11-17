using System;
using System.Collections.Generic;
using System.Linq;
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

using EmVerif.Model;

namespace EmVerif
{
    /// <summary>
    /// EditTabView.xaml の相互作用ロジック
    /// </summary>
    public partial class EditTabView : UserControl
    {
        public EditTabView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem == null)
            {
                return;
            }
            ViewModel.SelectedElement = (OneElement)((TreeView)sender).SelectedItem;
        }
    }
}
