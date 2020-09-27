using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class CalcFftCommand : IEmVerifCommand
    {
        public const UInt32 NoSelectValue = 0xFFFFFFFF;

        private string _StopState;

        private UInt32 _AdId = NoSelectValue;
        private UInt32 _PwmId = NoSelectValue;
        private UInt32 _SpioutId = NoSelectValue;
        private List<double> _DataList;

        private int _FftNum;
        private double _FreqDeltaUnit;
        private List<double> _InspectFreqList;
        private List<double> _InspectFreqDeltaList;
        private string _AmpResultArrayName;
        private string _PhaseResultArrayName;
        private string _OtherAmpResultVarName;

        private string _Message;

        public CalcFftCommand(
            string inStop,
            List<double> inInspectFreqList,
            List<double> inInspectFreqDeltaList,
            Int32 inFftNum,
            string inAmpResultArrayName,
            string inPhaseResultArrayName,
            string inOtherAmpResultVarName,
            UInt32 inAdId,
            UInt32 inSpioutId,
            UInt32 inPwmId,
            string inMessage
        )
        {
            _StopState = inStop;
            if (
                (inAdId < PublicConfig.AdChNum) &&
                (inSpioutId == NoSelectValue) &&
                (inPwmId == NoSelectValue)
            )
            {
                _AdId = inAdId;
            }
            else if (
                (inAdId == NoSelectValue) &&
                (inSpioutId < PublicConfig.SpioutChNum) &&
                (inPwmId == NoSelectValue)
            )
            {
                _SpioutId = inSpioutId;
            }
            else if (
                (inAdId == NoSelectValue) &&
                (inSpioutId == NoSelectValue) &&
                (inPwmId < PublicConfig.PwmChNum)
            )
            {
                _PwmId = inPwmId;
            }
            else
            {
                throw new Exception("AdId/SpioutId/PwmId は１チャネルのみしか選択できません。");
            }
            _DataList = new List<double>();

            _FftNum = inFftNum;
            _FreqDeltaUnit = (double)(Communication.PublicConfig.SamplingKhz * 1000) / (double)_FftNum;
            _InspectFreqList = new List<double>();
            if (inInspectFreqList != null)
            {
                foreach (var freq in inInspectFreqList)
                {
                    if (
                        (freq >= 0) &&
                        ((Communication.PublicConfig.SamplingKhz * 1000 / 2) > freq)
                    )
                    {
                        _InspectFreqList.Add(freq);
                    }
                    else
                    {
                        throw new Exception(
                            @"周波数設定は、0[Hz]以上" +
                            (Communication.PublicConfig.SamplingKhz * 1000 / 2).ToString() +
                            @"[Hz]未満です。"
                        );
                    }
                }
            }
            _InspectFreqDeltaList = new List<double>();
            if (inInspectFreqDeltaList == null)
            {
                _InspectFreqDeltaList = Enumerable.Repeat<double>(_FreqDeltaUnit, _InspectFreqList.Count).ToList();
            }
            else
            {
                for (int idx = 0; idx < inInspectFreqDeltaList.Count; idx++)
                {
                    if (inInspectFreqDeltaList[idx] < _FreqDeltaUnit)
                    {
                        throw new Exception("周波数デルタ設定が、最小分解能より小さい値です。");
                    }
                    else
                    {
                        _InspectFreqDeltaList.Add(inInspectFreqDeltaList[idx]);
                    }
                }
                for (int idx = inInspectFreqDeltaList.Count; idx < _InspectFreqList.Count; idx++)
                {
                    _InspectFreqDeltaList.Add(_FreqDeltaUnit);
                }
            }
            _AmpResultArrayName = inAmpResultArrayName;
            _PhaseResultArrayName = inPhaseResultArrayName;
            _OtherAmpResultVarName = inOtherAmpResultVarName;
            _Message = inMessage;
        }

        public void Boot(ControllerState ioState)
        {
            _DataList = new List<double>();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (ioState.CurrentState == _StopState)
            {
                outFinFlag = true;
            }
            else
            {
                if (_AdId != NoSelectValue)
                {
                    for (int smp = 0; smp < (ioState.CurrentAdDataList.Count / PublicConfig.AdChNum); smp++)
                    {
                        _DataList.Add(ioState.CurrentAdDataList[PublicConfig.AdChNum * smp + (int)_AdId]);
                    }
                }
                if (_SpioutId != NoSelectValue)
                {
                    for (int smp = 0; smp < (ioState.CurrentSpioutDataList.Count / PublicConfig.SpioutChNum); smp++)
                    {
                        _DataList.Add(ioState.CurrentSpioutDataList[PublicConfig.SpioutChNum * smp + (int)_SpioutId]);
                    }
                }
                if (_PwmId != NoSelectValue)
                {
                    for (int smp = 0; smp < (ioState.CurrentPwmDataList.Count / PublicConfig.PwmChNum); smp++)
                    {
                        _DataList.Add(ioState.CurrentPwmDataList[PublicConfig.PwmChNum * smp + (int)_PwmId]);
                    }
                }
                outFinFlag = false;
            }

            return null;
        }

        public void Finally(ControllerState ioState)
        {
            List<List<double>> ampEachRangeListList;
            List<double> averageAmpList;
            List<double> averagePhaseList;

            if (_Message != null)
            {
                LogManager.Instance.Set(_Message);
            }
            if (_DataList.Count < _FftNum)
            {
                LogManager.Instance.Set("\t" + @"データサンプル数が足りません。");
                return;
            }
            CalcFftAverage(out ampEachRangeListList, out averageAmpList, out averagePhaseList);
            SearchInfo(ampEachRangeListList, averageAmpList, averagePhaseList, ioState);
        }

        private void CalcFftAverage(out List<List<double>> outAmpEachRangeListList, out List<double> outAverageAmpList, out List<double> outAveragePhaseList)
        {
            int calcNum = 0;
            double[] averageReal = Enumerable.Repeat<double>(0, _FftNum / 2).ToArray();
            double[] averageImaginary = Enumerable.Repeat<double>(0, _FftNum / 2).ToArray();
            double[] averageAmp = Enumerable.Repeat<double>(0, _FftNum / 2).ToArray();

            outAmpEachRangeListList = new List<List<double>>();
            outAverageAmpList = new List<double>();
            outAveragePhaseList = new List<double>();
            while (_DataList.Count >= _FftNum)
            {
                Complex[] src = new Complex[_FftNum];
                Complex[] dst = new Complex[_FftNum];
                List<double> ampList = new List<double>();

                for (int idx = 0; idx < _FftNum; idx++)
                {
                    src[idx] = new Complex(_DataList[idx], 0);
                }
                _DataList.RemoveRange(0, _FftNum / 2);
                Utility.FourierTransform.Instance.Hamming(src);
                Utility.FourierTransform.Instance.Fft(src, out dst);

                for (int idx = 0; idx < (_FftNum / 2); idx++)
                {
                    double amp = dst[idx].Magnitude / 0.54 * 2 / _FftNum;

                    averageReal[idx] += dst[idx].Real;
                    averageImaginary[idx] += dst[idx].Imaginary;
                    ampList.Add(amp);
                    averageAmp[idx] += amp;
                }
                outAmpEachRangeListList.Add(ampList);
                calcNum++;
            }
            for (int idx = 0; idx < (_FftNum / 2); idx++)
            {
                outAverageAmpList.Add(averageAmp[idx] / calcNum);
                outAveragePhaseList.Add(Math.Atan2(averageImaginary[idx], averageReal[idx]) / Math.PI * 180);
            }
        }

        private void SearchInfo(List<List<double>> ioAmpEachRangeListList, List<double> inAverageAmpList, List<double> inAveragePhaseList, ControllerState ioState)
        {
            for (int idx = 0; idx < _InspectFreqList.Count; idx++)
            {
                double freqDeltaHalf = _InspectFreqDeltaList[idx] / 2;
                double freqRangeMin = _InspectFreqList[idx] - freqDeltaHalf;
                double freqRangeMax = _InspectFreqList[idx] + freqDeltaHalf;
                Int32 freqRangeMinIdx = (Int32)Math.Ceiling(freqRangeMin / _FreqDeltaUnit);
                Int32 freqRangeMaxIdx = (Int32)Math.Floor(freqRangeMax / _FreqDeltaUnit);
                Int32 maxFreqIdx = 0;
                double maxAmp = double.MinValue;
                double phase = 0;

                for (Int32 freqIdx = freqRangeMinIdx; freqIdx <= freqRangeMaxIdx; freqIdx++)
                {
                    if (maxAmp < inAverageAmpList[freqIdx])
                    {
                        maxAmp = inAverageAmpList[freqIdx];
                        phase = inAveragePhaseList[freqIdx];
                        maxFreqIdx = freqIdx;
                    }
                    foreach (var ampList in ioAmpEachRangeListList)
                    {
                        ampList[freqIdx] = double.MinValue;
                    }
                }
                LogManager.Instance.Set(
                    "\t" +
                    (maxFreqIdx * _FreqDeltaUnit).ToString() +
                    @"[Hz]の振幅は" +
                    maxAmp.ToString() +
                    @"、位相は" +
                    phase.ToString()
                );
                if (_AmpResultArrayName != null)
                {
                    string arrayName = _AmpResultArrayName + @"[" + idx.ToString() + "]";

                    ioState.VariableFormulaDict.Remove(arrayName);
                    ioState.VariableDict[arrayName] = Convert.ToDecimal(maxAmp);
                }
                if (_PhaseResultArrayName != null)
                {
                    string arrayName = _PhaseResultArrayName + @"[" + idx.ToString() + "]";

                    ioState.VariableFormulaDict.Remove(arrayName);
                    ioState.VariableDict[arrayName] = Convert.ToDecimal(phase);
                }
            }
            double otherAmpMax;
            Int32 otherFreqIdx;

            GetMaxAndIndex(ioAmpEachRangeListList, out otherAmpMax, out otherFreqIdx);
            LogManager.Instance.Set(
                "\t" + @"指定周波数以外の最大振幅は" +
                otherAmpMax.ToString() +
                @"で、周波数は" +
                (otherFreqIdx * _FreqDeltaUnit).ToString() +
                @"[Hz]"
            );
            if (_OtherAmpResultVarName != null)
            {
                string varName = _OtherAmpResultVarName;

                ioState.VariableFormulaDict.Remove(varName);
                ioState.VariableDict[varName] = Convert.ToDecimal(otherAmpMax);
            }
        }

        private void GetMaxAndIndex(List<List<double>> inAmpEachRangeListList, out double outMaxValue, out Int32 outIndex)
        {
            outMaxValue = double.MinValue;
            outIndex = 0;

            foreach (var ampList in inAmpEachRangeListList)
            {
                Int32 idx = 0;

                foreach (var amp in ampList)
                {
                    if (outMaxValue < amp)
                    {
                        outMaxValue = amp;
                        outIndex = idx;
                    }
                    idx++;
                }
            }
        }
    }
}
