using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class NoiseCheckCommand : IEmVerifCommand
    {
        private string _StopState;
        private List<Int32> _AdIdList;
        private UInt32 _SmpNum;
        private UInt32 _Order;
        private double _ThreshSigma;
        private double _Thresh;
        private string _Message;

        private List<double> _AdDataList;

        public NoiseCheckCommand(string inStop, string inAdId, UInt32 inSmpNum, UInt32 inOrder, double inThreshSigma, double inThresh, string inMessage)
        {
            _StopState = inStop;
            if (inAdId == null)
            {
                _AdIdList = Enumerable.Range(0, PublicConfig.AdChNum).ToList();
            }
            else
            {
                try
                {
                    _AdIdList = inAdId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
                }
                catch
                {
                    throw new Exception("AdId 設定に間違い有⇒" + inAdId);
                }
            }
            foreach (var id in _AdIdList)
            {
                if ((id >= PublicConfig.AdChNum) || (id < 0))
                {
                    throw new Exception("AdId 設定は0以上" + PublicConfig.AdChNum + "未満");
                }
            }
            if (inSmpNum <= 1)
            {
                throw new Exception("サンプル数は2以上");
            }
            if ((inThresh <= 0) || (inThreshSigma <= 0))
            {
                throw new Exception("閾値は0より大");
            }
            _SmpNum = inSmpNum;
            _Order = inOrder;
            _ThreshSigma = inThreshSigma;
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
            UInt32 minSmpNum = _SmpNum + 2 * _Order;
            string log;

            if (smpNum < minSmpNum)
            {
                log = "\tノイズチェックするには、サンプル数不足⇒NG";
            }
            else
            {
                log = CheckNoise();
            }
            if (_Message != null)
            {
                LogManager.Instance.Set(_Message);
            }
            LogManager.Instance.Set(log);
        }

        private string CheckNoise()
        {
            string ret = "";
            var dataPerChList = MakeDataPerCh();
            int chIdx = 0;

            foreach (var dataList in dataPerChList)
            {
                List<double> checkDataList = new List<double>(dataList);

                for (UInt32 idx = 0; idx < _Order; idx++)
                {
                    checkDataList = CalcDiff(checkDataList);
                }
                for (int idx = 0; idx <= (checkDataList.Count - _SmpNum); idx++)
                {
                    List<double> checkDataDivList = checkDataList.GetRange(idx, (int)_SmpNum);
                    double maxAbs = checkDataDivList.Select(x => Math.Abs(x)).Max();

                    if (maxAbs >= _Thresh)
                    {
                        double oneSigma = CalcOneSigma(checkDataDivList);

                        if (oneSigma != 0)
                        {
                            double average = CalcAverage(checkDataDivList);
                            double checkData = checkDataDivList.Last();
                            double sigma = Math.Abs((checkData - average) / oneSigma);

                            if (sigma >= _ThreshSigma)
                            {
                                ret = ret + "\tAd" + _AdIdList[chIdx] + "の" + (idx + _SmpNum + _Order) + "サンプル位置にノイズが存在⇒NG\r\n";
                            }
                        }
                    }
                }
                chIdx++;
            }

            if (ret == "")
            {
                ret = "\tOK";
            }

            return ret;
        }

        private List<List<double>> MakeDataPerCh()
        {
            Int32 smpNum = _AdDataList.Count / PublicConfig.AdChNum;
            List<List<double>> dataPerChList = new List<List<double>>();

            _AdIdList.ForEach(x => dataPerChList.Add(new List<double>()));
            for (int smp = 0; smp < smpNum; smp++)
            {
                int ch = 0;

                foreach (var id in _AdIdList)
                {
                    dataPerChList[ch].Add(_AdDataList[PublicConfig.AdChNum * smp + id]);
                    ch++;
                }
            }

            return dataPerChList;
        }

        private List<double> CalcDiff(IReadOnlyList<double> inDataList)
        {
            List<double> ret = new List<double>();

            for (int idx = 0; idx < (inDataList.Count - 2); idx++)
            {
                double diff = (inDataList[idx + 2] - inDataList[idx]) / 2;

                ret.Add(diff);
            }

            return ret;
        }

        private double CalcOneSigma(IReadOnlyList<double> inDataList)
        {
            double sigma;
            double sum = 0;
            double squareSum = 0;
            Int32 count = inDataList.Count;

            foreach (var data in inDataList)
            {
                sum += data;
                squareSum += data * data;
            }
            sigma = Math.Sqrt(((count * squareSum) - (sum * sum)) / (count * (count - 1)));

            return sigma;
        }

        private double CalcAverage(IReadOnlyList<double> inDataList)
        {
            double ret = 0;

            foreach (var data in inDataList)
            {
                ret += data;
            }
            ret = ret / inDataList.Count;

            return ret;
        }
    }
}
