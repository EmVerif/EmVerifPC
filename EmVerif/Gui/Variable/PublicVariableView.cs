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
    public partial class PublicVariableView : UserControl
    {
        private object _NameToValueDictLock = new object();
        private Dictionary<string, Decimal> _NameToValueDict = new Dictionary<string, Decimal>();
        private Dictionary<string, string> _NameToFormulaDict = new Dictionary<string, string>();
        private Dictionary<string, Decimal> _PrevNameToValueDict = new Dictionary<string, Decimal>();
        private Dictionary<string, OneCell> _NameToCellDict = new Dictionary<string, OneCell>();

        private object _NameToUpdatedValueDictLock = new object();
        private Dictionary<string, Decimal> _NameToUpdatedValueDict = new Dictionary<string, decimal>();

        public PublicVariableView()
        {
            InitializeComponent();
            tm_UpdateVariableView.Interval = 100;
            tm_UpdateVariableView.Start();
        }

        public IReadOnlyDictionary<string, Decimal> GetUpdatedValue()
        {
            IReadOnlyDictionary<string, Decimal> ret;

            lock (_NameToUpdatedValueDictLock)
            {
                ret = _NameToUpdatedValueDict;
                _NameToUpdatedValueDict = new Dictionary<string, decimal>();
            }

            return ret;
        }

        public void Set(Dictionary<string, Decimal> inNameToValueDic, Dictionary<string, string> inNameToFormulaDic)
        {
            lock (_NameToValueDictLock)
            {
                _NameToValueDict = new Dictionary<string, decimal>(inNameToValueDic);
                _NameToFormulaDict = new Dictionary<string, string>(inNameToFormulaDic);
            }
        }

        private void tm_UpdateVariableView_Tick(object sender, EventArgs e)
        {
            Dictionary<string, Decimal> nameToValueDict;
            Dictionary<string, string> nameToFormulaDict;

            lock (_NameToValueDictLock)
            {
                nameToValueDict = _NameToValueDict;
                nameToFormulaDict = _NameToFormulaDict;
            }

            foreach (var varName in nameToValueDict.Keys)
            {
                if (!_NameToCellDict.ContainsKey(varName))
                {
                    OneCell oneCell = new OneCell();

                    if (nameToFormulaDict.ContainsKey(varName))
                    {
                        oneCell.tb_Value.ReadOnly = true;
                        oneCell.tb_Value.Text = nameToValueDict[varName].ToString() + @"(" + nameToFormulaDict[varName] + @")";
                    }
                    else
                    {
                        oneCell.tb_Value.Text = nameToValueDict[varName].ToString();
                    }
                    oneCell.tb_VarName.Text = varName;
                    flowLayoutPanel1.Controls.Add(oneCell);
                    _NameToCellDict.Add(varName, oneCell);
                }
                else if (_PrevNameToValueDict[varName] != nameToValueDict[varName])
                {
                    if (nameToFormulaDict.ContainsKey(varName))
                    {
                        _NameToCellDict[varName].tb_Value.Text = nameToValueDict[varName].ToString() + @"(" + nameToFormulaDict[varName] + @")";
                    }
                    else
                    {
                        _NameToCellDict[varName].tb_Value.Text = nameToValueDict[varName].ToString();
                    }
                }
            }
            _PrevNameToValueDict = nameToValueDict;
        }

        private void bu_Set_Click(object sender, EventArgs e)
        {
            Dictionary<string, decimal> nameToUpdatedValueDict = new Dictionary<string, decimal>();

            foreach (var varName in _NameToCellDict.Keys)
            {
                if (_NameToCellDict[varName].ValueChangedFlag)
                {
                    try
                    {
                        nameToUpdatedValueDict.Add(varName, Convert.ToDecimal(_NameToCellDict[varName].tb_Value.Text));
                    }
                    catch
                    {
                        _NameToCellDict[varName].tb_Value.Text = _PrevNameToValueDict[varName].ToString();
                    }
                    _NameToCellDict[varName].tb_Value.BackColor = Color.White;
                    _NameToCellDict[varName].ValueChangedFlag = false;
                }
            }
            lock (_NameToUpdatedValueDictLock)
            {
                _NameToUpdatedValueDict = nameToUpdatedValueDict;
            }
        }
    }
}
