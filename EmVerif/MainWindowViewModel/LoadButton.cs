using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using EmVerif.Model;

namespace EmVerif.MainWindowViewModel
{
    class LoadButton : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public event EventHandler LoadFinished;

        public LoadButton()
        {
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
                    Database.Instance.Load(ofd.FileName);
                    LoadFinished?.Invoke(this, new EventArgs());
                    MessageBox.Show("OK");
                }
                catch
                {
                    LoadFinished?.Invoke(this, new EventArgs());
                    MessageBox.Show("NG");
                }
            }
        }
    }
}
