using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmVerif.Gui.Variable
{
    public partial class OneCell : UserControl
    {
        public Boolean ValueChangedFlag;

        public OneCell()
        {
            InitializeComponent();
            ValueChangedFlag = false;
        }

        private void tb_Value_Leave(object sender, EventArgs e)
        {
            tb_Value.BackColor = Color.Red;
            ValueChangedFlag = true;
        }
    }
}
