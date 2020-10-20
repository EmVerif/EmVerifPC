﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class FadeCommand : IEmVerifCommand
    {
        private string _VarName;
        private Decimal _Target;
        private double _TimeMs;
        private string _Next;

        private double _StartTimeMs;
        private Decimal _StartValue;

        public FadeCommand(
            string inVarName,
            Decimal inTarget,
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
                _StartValue = ioState.VariableDict[_VarName];
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
                ioState.VariableDict[_VarName] = _Target;
                outFinFlag = true;
            }
            else
            {
                ioState.VariableDict[_VarName] = _StartValue + (_Target - _StartValue) * (Decimal)(pastTimeMs / _TimeMs);
                outFinFlag = false;
            }

            return _Next;
        }

        public void Finally(ControllerState inState)
        {
        }
    }
}
