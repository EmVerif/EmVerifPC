using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;

namespace EmVerif.MainWindowViewModel
{
    class LoadButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public LoadButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "ファイル(*.xml)|*.xml|すべてのファイル(*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<OneElement>));

                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        _RefViewModel.TreeViewList = xmlSerializer.Deserialize(fs) as ObservableCollection<OneElement>;
                        SetParent();
                        _RefViewModel.OnPropertyChanged("TreeViewList");
                    }
                    MessageBox.Show("OK");
                }
                catch
                {
                    _RefViewModel.TreeViewList = new ObservableCollection<OneElement>();
                    _RefViewModel.OnPropertyChanged("TreeViewList");
                    MessageBox.Show("NG");
                }
            }
        }

        private void SetParent()
        {
            foreach (var treeView in _RefViewModel.TreeViewList)
            {
                treeView.SetParent(null);
            }
        }
    }
}
