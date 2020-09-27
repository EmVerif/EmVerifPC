﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class ValueCheckCommand : IEmVerifCommand
    {
        private string _CheckValueStr;
        private Decimal _ExpValueMax;
        private Decimal _ExpValueMin;
        private string _Message;
        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_][\w\[\]]*)");

        public ValueCheckCommand(
            string inCheckValue,
            Decimal inExpValueMax,
            Decimal inExpValueMin,
            string inMessage
        )
        {
            _CheckValueStr = inCheckValue;
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
            Decimal checkValue = ConvertFormula(inState, _CheckValueStr);
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
            LogManager.Instance.Set("\t" + _ExpValueMin.ToString() + @" <= " + _CheckValueStr + @" <= " + _ExpValueMax.ToString() + @"⇒" + result);
        }

        private Decimal ConvertFormula(ControllerState inState, string inOrgFormula)
        {
            DataTable dt = new DataTable();
            var varNameMatches = _VarNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    resultStr = resultStr.Replace(varName, inState.VariableDict[varName].ToString());
                }
            }

            return Convert.ToDecimal(dt.Compute(resultStr, ""));
        }
    }
}