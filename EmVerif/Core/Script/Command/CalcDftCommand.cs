using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class CalcDftCommand : IEmVerifCommand
    {
        public const UInt32 NoSelectValue = 0xFFFFFFFF;

        private string _StopState;

        private UInt32 _AdId = NoSelectValue;
        private UInt32 _PwmId = NoSelectValue;
        private UInt32 _SpioutId = NoSelectValue;
        private List<double> _DataList;

        private List<double> _InspectFreqList;
        private string _AmpResultArrayName;
        private string _PhaseResultArrayName;

        private string _Message;

        public CalcDftCommand(
            string inStop,
            List<double> inInspectFreqList,
            string inAmpResultArrayName,
            string inPhaseResultArrayName,
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

            _InspectFreqList = new List<double>();
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
            _AmpResultArrayName = inAmpResultArrayName;
            _PhaseResultArrayName = inPhaseResultArrayName;
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
            if (_DataList.Count == 0)
            {
                return;
            }
            if (_Message != null)
            {
                LogManager.Instance.Set(_Message);
            }

            List<Complex> srcList = new List<Complex>();
            Complex[] srcArray;
            foreach (var data in _DataList)
            {
                srcList.Add(new Complex(data, 0));
            }
            srcArray = srcList.ToArray();
            Utility.FourierTransform.Instance.Hamming(srcArray, true);

            int idx = 0;
            foreach (var freq in _InspectFreqList)
            {
                Complex dst;
                double amp, phase;

                Utility.FourierTransform.Instance.Dft(srcArray, freq / Communication.PublicConfig.SamplingKhz / 1000, out dst);
                amp = dst.Magnitude * 2 / _DataList.Count;
                phase = dst.Phase / Math.PI * 180;
                LogManager.Instance.Set(
                    "\t" +
                    freq.ToString() +
                    @"[Hz]の振幅は" +
                    amp.ToString() +
                    @"、位相は" +
                    phase.ToString() +
                    @"[deg]"
                );
                if (_AmpResultArrayName != null)
                {
                    string arrayName = _AmpResultArrayName + @"[" + idx.ToString() + "]";

                    ioState.VariableFormulaDict.Remove(arrayName);
                    ioState.VariableDict[arrayName] = Convert.ToDecimal(amp);
                }
                if (_PhaseResultArrayName != null)
                {
                    string arrayName = _PhaseResultArrayName + @"[" + idx.ToString() + "]";

                    ioState.VariableFormulaDict.Remove(arrayName);
                    ioState.VariableDict[arrayName] = Convert.ToDecimal(phase);
                }
                idx++;
            }
        }
    }
}
