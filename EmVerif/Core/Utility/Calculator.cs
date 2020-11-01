using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmVerif.Core.Utility
{
    class Calculator
    {
        public static Calculator Instance = new Calculator();

        private DataTable _Dt = new DataTable();
        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_][\w\[\]]*)");

        public Decimal ConvertFormula(in string inOrgFormula, in Dictionary<string, Decimal> inVariableDict)
        {
            var varNameMatches = _VarNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    try
                    {
                        resultStr = resultStr.Replace(varName, inVariableDict[varName].ToString());
                    }
                    catch
                    {
                        throw new Exception("変数" + varName + "が見つかりません。⇒NG");
                    }
                }
            }

            return Convert.ToDecimal(_Dt.Compute(resultStr, ""));
        }
    }
}
