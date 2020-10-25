using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class ByteArrayCheckContainsCommand : IEmVerifCommand
    {
        private string _ArrayName;
        private IReadOnlyList<byte> _ExpValueList;
        private IReadOnlyList<byte> _MaskList;
        private bool _MatchCheckFlag;
        private string _Message;

        public ByteArrayCheckContainsCommand(string inArrayName, IReadOnlyList<byte> inExpValue, IReadOnlyList<byte> inMask, bool inMatchCheckFlag, string inMessage)
        {
            List<Byte> maskList = new List<byte>();

            _ArrayName = inArrayName;
            _Message = inMessage;
            if (inMask != null)
            {
                maskList = new List<Byte>(inMask);
            }
            if (maskList.Count < inExpValue.Count)
            {
                maskList.AddRange(Enumerable.Repeat<Byte>(0xFF, (inExpValue.Count - maskList.Count)));
            }
            _MatchCheckFlag = inMatchCheckFlag;
            _ExpValueList = inExpValue;
            _MaskList = maskList;
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
            List<Byte> checkValueList;
            string result = "\tOK";
            Boolean isMatch = false;

            try
            {
                checkValueList = GetValueList(inState);
                for (int baseIdx = 0; baseIdx < (checkValueList.Count - _ExpValueList.Count); baseIdx++)
                {
                    Boolean isNotMatch = false;

                    for (int idx = 0; idx < _ExpValueList.Count; idx++)
                    {
                        if ((checkValueList[baseIdx] & _MaskList[baseIdx]) != _ExpValueList[baseIdx])
                        {
                            isNotMatch = true;
                            break;
                        }
                    }

                    if (!isNotMatch)
                    {
                        isMatch = true;
                        break;
                    }
                }

                if (_MatchCheckFlag)
                {
                    if (isMatch)
                    {
                        result = "\tOK";
                    }
                    else
                    {
                        result = "\tNG";
                    }
                }
                else
                {
                    if (isMatch)
                    {
                        result = "\tNG";
                    }
                    else
                    {
                        result = "\tOK";
                    }
                }
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            if (_Message != null)
            {
                LogManager.Instance.Set(_Message);
            }
            LogManager.Instance.Set(result);
        }

        private List<byte> GetValueList(ControllerState inState)
        {
            int idx = 0;
            List<Byte> ret = new List<byte>();

            while (true)
            {
                string arrayName = _ArrayName + @"[" + idx.ToString() + @"]";
                Decimal value;

                if (!inState.VariableDict.ContainsKey(arrayName))
                {
                    break;
                }
                value = inState.VariableDict[arrayName];
                if (
                    (((Decimal)(Byte)value - value) != 0) ||
                    (value < Byte.MinValue) ||
                    (value > Byte.MaxValue)
                )
                {
                    throw new Exception("\t" + _ArrayName + "は、バイトフォーマットではありません。⇒NG");
                }
                ret.Add((Byte)value);
                idx++;
            }

            return ret;
        }
    }
}
