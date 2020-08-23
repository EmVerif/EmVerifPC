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
    class SaveButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModel _RefViewModel;

        public SaveButton(ViewModel vm)
        {
            _RefViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "ファイル(*.xml)|*.xml|すべてのファイル(*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<OneElement>));

                    using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        xmlSerializer.Serialize(sw, _RefViewModel.TreeViewList);
                        sw.Flush();
                    }
                    MessageBox.Show("OK");
                }
                catch
                {
                    MessageBox.Show("NG");
                }
            }
        }
    }
}
