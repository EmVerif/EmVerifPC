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
    class SaveButton : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public SaveButton()
        {
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
                    Database.Instance.Save(sfd.FileName);
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
