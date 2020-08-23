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
        private List<Int32> _AdIdList;
        private List<double> _AdDataList;
        private double _Thresh;
        private string _Message;

        public SilenceCheckCommand(string inStop, string inAdId, double inThresh, string inMessage)
        {
            _StopState = inStop;
            _AdDataList = new List<double>();
            if (inAdId != null)
            {
                _AdIdList = inAdId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            }
            _AdIdList = new List<Int32>();
            _Thresh = inThresh;
            _Message = inMessage;
        }

        public void Boot(ControllerState inState)
        {
            _AdDataList = new List<double>();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (ioState.CurrentState == _StopState)
            {
                outFinFlag = true;
            }
            else
            {
                _AdDataList.AddRange(ioState.CurrentAdDataList);
                outFinFlag = false;
            }

            return null;
        }

        public void Finally(ControllerState inState)
        {
            Int32 smpNum = _AdDataList.Count / PublicConfig.AdChNum;
            List<double> dataList = new List<double>();
            Boolean isSilent = true;

            for (int smp = 0; smp < smpNum; smp++)
            {
                foreach (var id in _AdIdList)
                {
                    if (Math.Abs(_AdDataList[PublicConfig.AdChNum * smp + (int)id]) >= _Thresh)
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
