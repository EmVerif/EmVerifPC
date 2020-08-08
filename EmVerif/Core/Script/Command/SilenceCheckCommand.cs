using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class SilenceCheckCommand : IEmVerifCommand
    {
        private string _StopState;
        private List<Int32> _InIdList;
        private List<double> _InDataList;
        private double _Thresh;
        private string _Message;

        public SilenceCheckCommand(string inStop, string inInId, double inThresh, string inMessage)
        {
            _StopState = inStop;
            _InDataList = new List<double>();
            if (inInId != null)
            {
                _InIdList = inInId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            }
            _InIdList = new List<Int32>();
            _Thresh = inThresh;
            _Message = inMessage;
        }

        public void Boot(ControllerState inState)
        {
            _InDataList = new List<double>();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (ioState.CurrentState == _StopState)
            {
                outFinFlag = true;
            }
            else
            {
                _InDataList.AddRange(ioState.CurrentInDataList);
                outFinFlag = false;
            }

            return null;
        }

        public void Finally()
        {
            Int32 smpNum = _InDataList.Count / PublicConfig.InChNum;
            List<double> dataList = new List<double>();
            Boolean isSilent = true;

            for (int smp = 0; smp < smpNum; smp++)
            {
                foreach (var id in _InIdList)
                {
                    if (Math.Abs(_InDataList[PublicConfig.InChNum * smp + (int)id]) >= _Thresh)
                    {
                        isSilent = false;
                        break;
                    }
                }
            }
            if (isSilent)
            {
                LogManager.Instance.Set(_Message + "OK");
            }
            else
            {
                LogManager.Instance.Set(_Message + "NG");
            }
        }
    }
}
