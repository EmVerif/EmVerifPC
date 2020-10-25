using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class FadeCommand : IEmVerifCommand
    {
        private string _VarName;
        private double _Target;
        private double _TimeMs;
        private string _Next;

        private double _StartTimeMs;
        private double _StartValue;

        public FadeCommand(
            string inVarName,
            double inTarget,
            double inTime,
            string inNext = null
        )
        {
            _VarName = inVarName;
            _Target = inTarget;
            _TimeMs = inTime * 1000;
            _Next = inNext;
        }

        public void Boot(ControllerState ioState)
        {
            _StartTimeMs = ioState.TimestampMs;
            try
            {
                _StartValue = (double)ioState.VariableDict[_VarName];
            }
            catch
            {
                throw new Exception("変数" + _VarName + "が見つかりません。⇒NG");
            }
            ioState.VariableFormulaDict.Remove(_VarName);
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            double pastTimeMs = ioState.TimestampMs - _StartTimeMs;

            if (pastTimeMs >= _TimeMs)
            {
                ioState.VariableDict[_VarName] = (Decimal)_Target;
                outFinFlag = true;
            }
            else
            {
                ioState.VariableDict[_VarName] = (Decimal)(_StartValue + (_Target - _StartValue) * (pastTimeMs / _TimeMs));
                outFinFlag = false;
            }

            return _Next;
        }

        public void Finally(ControllerState inState)
        {
        }
    }
}
