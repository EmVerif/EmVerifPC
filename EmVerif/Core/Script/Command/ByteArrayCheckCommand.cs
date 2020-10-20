using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class ByteArrayCheckCommand : IEmVerifCommand
    {
        private string _ArrayName;
        private IReadOnlyList<Byte> _ExpValueList;
        private IReadOnlyList<Byte> _MaskList;
        private string _Message;
        public ByteArrayCheckCommand(string inArrayName, IReadOnlyList<byte> inExpValue, IReadOnlyList<byte> inMask, string inMessage)
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
            
            try
            {
                checkValueList = GetValueList(inState);
                if (checkValueList.Count != _ExpValueList.Count)
                {
                    result = "\t配列数が不一致。" + _ArrayName + "は、配列数が" + checkValueList.Count + "個存在。⇒NG";
                }
                else
                {
                    for (int idx = 0; idx < checkValueList.Count; idx++)
                    {
                        if ((checkValueList[idx] & _MaskList[idx]) != _ExpValueList[idx])
                        {
                            result = "\t" +
                                _ArrayName + @"[" + idx + @"] = 0x" +
                                checkValueList[idx].ToString("X2") +
                                @" & 0x" +
                                _MaskList[idx].ToString("X2") +
                                " が、期待値 0x" +
                                _ExpValueList[idx].ToString("X2") +
                                "と不一致。⇒NG";
                            break;
                        }
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
