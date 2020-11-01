using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using EmVerif.Core.Utility;

namespace EmVerif.Core.Script.Command
{
    class ValueCheckCommand : IEmVerifCommand
    {
        private string _FormulaStr;
        private double _ExpValueMax;
        private double _ExpValueMin;
        private string _Message;

        public ValueCheckCommand(
            string inFormula,
            double inExpValueMax,
            double inExpValueMin,
            string inMessage
        )
        {
            _FormulaStr = inFormula;
            _ExpValueMax = inExpValueMax;
            _ExpValueMin = inExpValueMin;
            _Message = inMessage;
        }

        public void Boot(ControllerState ioState)
        {
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            outFinFlag = true;

            return null;
        }

        public void Finally(ControllerState inState)
        {
            double checkValue = ConvertFormula(inState, _FormulaStr);
            string result;

            if (
                (_ExpValueMax >= checkValue) &&
                (_ExpValueMin <= checkValue)
            )
            {
                result = @"OK";
            }
            else
            {
                result = @"NG";
            }
            if (_Message != null)
            {
                LogManager.Instance.Set(_Message);
            }
            LogManager.Instance.Set("\t" + _ExpValueMin.ToString() + @" <= " + _FormulaStr + @" <= " + _ExpValueMax.ToString() + @"⇒" + result);
        }

        private double ConvertFormula(ControllerState inState, string inOrgFormula)
        {
            return Convert.ToDouble(Calculator.Instance.ConvertFormula(inOrgFormula, inState.VariableDict));
        }
    }
}
