using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class SetCommand : IEmVerifCommand
    {
        private class VirtualPath
        {
            public enum PathType
            {
                AdToPwm,
                AdToSpiout,
                SpioutToPwm
            }

            public PathType Type;
            public Int32 SourceStartId;
            public Int32 SourceEndId;
            public Int32 DstStartId;
            public Int32 DstEndId;
            public float Gain;
            public Byte Delay;

            private const string _AdToPwmRegexPattern = @"Ad(?<SrcId>\d+|\*)_Pwm(?<DstId>\d+|\*)";
            private const string _AdToSpioutRegexPattern = @"Ad(?<SrcId>\d+|\*)_Spiout(?<DstId>\d+|\*)";
            private const string _SpioutToPwmRegexPattern = @"Spiout(?<SrcId>\d+|\*)_Pwm(?<DstId>\d+|\*)";

            public VirtualPath(PublicApis.VirtualPath inVirtualPath)
            {
                var adToPwmRegex = new Regex(_AdToPwmRegexPattern);
                var adToSpioutRegex = new Regex(_AdToSpioutRegexPattern);
                var spioutToPwmRegex = new Regex(_SpioutToPwmRegexPattern);
                var adToPwmMatch = adToPwmRegex.Match(inVirtualPath.Path);
                var adToSpioutMatch = adToSpioutRegex.Match(inVirtualPath.Path);
                var spioutToPwmMatch = spioutToPwmRegex.Match(inVirtualPath.Path);

                Gain = inVirtualPath.Gain;
                Delay = inVirtualPath.Delay;
                if (adToPwmMatch.Success)
                {
                    Type = PathType.AdToPwm;
                    if ((string)adToPwmMatch.Groups["SrcId"].Value == @"*")
                    {
                        SourceStartId = 0;
                        SourceEndId = PublicConfig.AdChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(adToPwmMatch.Groups["SrcId"].Value);

                        if (id < PublicConfig.AdChNum)
                        {
                            SourceStartId = id;
                            SourceEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Ad の設定範囲は、0 以上 " + PublicConfig.AdChNum.ToString() + " 未満。"
                            );
                        }
                    }
                    if ((string)adToPwmMatch.Groups["DstId"].Value == @"*")
                    {
                        DstStartId = 0;
                        DstEndId = PublicConfig.PwmChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(adToPwmMatch.Groups["DstId"].Value);

                        if (id < PublicConfig.PwmChNum)
                        {
                            DstStartId = id;
                            DstEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Pwm の設定範囲は、0 以上 " + PublicConfig.PwmChNum.ToString() + " 未満。"
                            );
                        }
                    }
                }
                else if (adToSpioutMatch.Success)
                {
                    Type = PathType.AdToSpiout;
                    if ((string)adToSpioutMatch.Groups["SrcId"].Value == @"*")
                    {
                        SourceStartId = 0;
                        SourceEndId = PublicConfig.AdChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(adToSpioutMatch.Groups["SrcId"].Value);

                        if (id < PublicConfig.AdChNum)
                        {
                            SourceStartId = id;
                            SourceEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Ad の設定範囲は、0 以上 " + PublicConfig.AdChNum.ToString() + " 未満。"
                            );
                        }
                    }
                    if ((string)adToSpioutMatch.Groups["DstId"].Value == @"*")
                    {
                        DstStartId = 0;
                        DstEndId = PublicConfig.SpioutChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(adToSpioutMatch.Groups["DstId"].Value);

                        if (id < PublicConfig.SpioutChNum)
                        {
                            DstStartId = id;
                            DstEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Spiout の設定範囲は、0 以上 " + PublicConfig.SpioutChNum.ToString() + " 未満。"
                            );
                        }
                    }
                }
                else if (spioutToPwmMatch.Success)
                {
                    Type = PathType.SpioutToPwm;
                    if ((string)spioutToPwmMatch.Groups["SrcId"].Value == @"*")
                    {
                        SourceStartId = 0;
                        SourceEndId = PublicConfig.SpioutChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(spioutToPwmMatch.Groups["SrcId"].Value);

                        if (id < PublicConfig.SpioutChNum)
                        {
                            SourceStartId = id;
                            SourceEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Spiout の設定範囲は、0 以上 " + PublicConfig.SpioutChNum.ToString() + " 未満。"
                            );
                        }
                    }
                    if ((string)spioutToPwmMatch.Groups["DstId"].Value == @"*")
                    {
                        DstStartId = 0;
                        DstEndId = PublicConfig.PwmChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(spioutToPwmMatch.Groups["DstId"].Value);

                        if (id < PublicConfig.PwmChNum)
                        {
                            DstStartId = id;
                            DstEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "Pwm の設定範囲は、0 以上 " + PublicConfig.PwmChNum.ToString() + " 未満。"
                            );
                        }
                    }
                }
                else
                {
                    throw new Exception(
                        "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                        "Ad0_Pwm0、Spiout*_Pwm2、Ad1_Spiout* というように記述する。"
                    );
                }
            }
        }

        private string _NextState;
        private PublicApis.Signal _Signal;
        private VirtualPath _VirtualPath;
        private PublicApis.SquareWave _SquareWave;
        private PublicApis.SetVar _SetVar;
        private bool _IsNumeric;
        private Decimal _Value;
        private Regex _NumericRegex = new Regex(@"^\s*[0-9]+\.?[0-9]*\s*$");

        public SetCommand(
            PublicApis.Signal inSignal,
            PublicApis.VirtualPath inVirtualPath,
            PublicApis.SquareWave inSquareWave,
            PublicApis.SetVar inSetVar,
            string inNextState
        )
        {
            _NextState = inNextState;
            _Signal = inSignal;
            if (inVirtualPath != null)
            {
                _VirtualPath = new VirtualPath(inVirtualPath);
            }
            else
            {
                _VirtualPath = null;
            }
            _SquareWave = inSquareWave;
            _SetVar = inSetVar;
            if (inSetVar != null)
            {
                _IsNumeric = _NumericRegex.IsMatch(_SetVar.Value);
                if (_IsNumeric)
                {
                    _Value = Convert.ToDecimal(_SetVar.Value);
                }
            }

            CheckSignalParam();
            // CheckVirtualPathParam(); VirtualPathクラスのコンストラクタで実施済み。
            // CheckSquareWave(); チェックするパラメータ無し。
        }

        public void Boot(ControllerState inState)
        {
        }

        public string ExecPer10Ms(ControllerState ioState, out Boolean outFinFlag)
        {
            ExecSignalSetting(ioState);
            ExecVirtualPathSetting(ioState);
            ExecSquareWaveSetting(ioState);
            ExecSetVarSetting(ioState);
            outFinFlag = true;

            return _NextState;
        }

        public void Finally(ControllerState inState)
        {
        }

        private void CheckSignalParam()
        {
            if (_Signal != null)
            {
                switch (_Signal.GetType().Name)
                {
                    case "PwmSineWave":
                        {
                            if (_Signal.Ch >= PublicConfig.PwmChNum)
                            {
                                throw new Exception(
                                    "Set コマンド内 Signal.Ch 設定エラー⇒" + _Signal.Ch + "\n" +
                                    "Ch 設定値範囲は 0 以上 " + PublicConfig.PwmChNum + " 未満。"
                                );
                            }
                            PublicApis.PwmSineWave signal = (PublicApis.PwmSineWave)_Signal;
                            if (signal.Id >= PublicConfig.SignalBaseNum)
                            {
                                throw new Exception(
                                    "Set コマンド内 Signal.Id 設定エラー⇒" + signal.Id + "\n" +
                                    "Id 設定値範囲は 0 以上 " + PublicConfig.SignalBaseNum + " 未満。"
                                );
                            }
                        }
                        break;
                    case "PwmWhiteNoise":
                        if (_Signal.Ch >= PublicConfig.PwmChNum)
                        {
                            throw new Exception(
                                "Set コマンド内 Signal.Ch 設定エラー⇒" + _Signal.Ch + "\n" +
                                "Ch 設定値範囲は 0 以上 " + PublicConfig.PwmChNum + " 未満。"
                            );
                        }
                        break;
                    case "SpioutSineWave":
                        {
                            if (_Signal.Ch >= PublicConfig.SpioutChNum)
                            {
                                throw new Exception(
                                    "Set コマンド内 Signal.Ch 設定エラー⇒" + _Signal.Ch + "\n" +
                                    "Ch 設定値範囲は 0 以上 " + PublicConfig.SpioutChNum + " 未満。"
                                );
                            }
                            PublicApis.SpioutSineWave signal = (PublicApis.SpioutSineWave)_Signal;
                            if (signal.Id >= PublicConfig.SignalBaseNum)
                            {
                                throw new Exception(
                                    "Set コマンド内 Signal.Id 設定エラー⇒" + signal.Id + "\n" +
                                    "Id 設定値範囲は 0 以上 " + PublicConfig.SignalBaseNum + " 未満。"
                                );
                            }
                        }
                        break;
                    case "SpioutWhiteNoise":
                        if (_Signal.Ch >= PublicConfig.SpioutChNum)
                        {
                            throw new Exception(
                                "Set コマンド内 Signal.Ch 設定エラー⇒" + _Signal.Ch + "\n" +
                                "Ch 設定値範囲は 0 以上 " + PublicConfig.SpioutChNum + " 未満。"
                            );
                        }
                        break;
                    default:
                        throw new Exception(
                            "不明な信号タイプ⇒" + _Signal.GetType().Name + "\n" +
                            "使用可能なクラスは、PwmSineWave/PwmWhiteNoise/SpioutSineWave/SpioutWhiteNoise。"
                        );
                }
            }
        }

        private void ExecSignalSetting(ControllerState ioState)
        {
            if (_Signal != null)
            {
                switch (_Signal.GetType().Name)
                {
                    case "PwmSineWave":
                        {
                            PublicApis.PwmSineWave signal = (PublicApis.PwmSineWave)_Signal;
                            int idx = PublicConfig.SignalBaseNum * signal.Ch + signal.Id;

                            ioState.PwmSineHz[idx] = signal.Freq;
                            ioState.PwmSineGain[idx] = signal.Gain;
                            ioState.PwmSinePhase[idx] = signal.Phase;
                        }
                        break;
                    case "PwmWhiteNoise":
                        {
                            PublicApis.PwmWhiteNoise signal = (PublicApis.PwmWhiteNoise)_Signal;
                            int idx = signal.Ch;

                            ioState.PwmWhiteNoiseGain[idx] = signal.Gain;
                        }
                        break;
                    case "SpioutSineWave":
                        {
                            PublicApis.SpioutSineWave signal = (PublicApis.SpioutSineWave)_Signal;
                            int idx = PublicConfig.SignalBaseNum * signal.Ch + signal.Id;

                            ioState.SpioutSineHz[idx] = signal.Freq;
                            ioState.SpioutSineGain[idx] = signal.Gain;
                            ioState.SpioutSinePhase[idx] = signal.Phase;
                        }
                        break;
                    case "SpioutWhiteNoise":
                        {
                            PublicApis.SpioutWhiteNoise signal = (PublicApis.SpioutWhiteNoise)_Signal;
                            int idx = signal.Ch;

                            ioState.SpioutWhiteNoiseGain[idx] = signal.Gain;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void ExecVirtualPathSetting(ControllerState ioState)
        {
            if (_VirtualPath != null)
            {
                switch (_VirtualPath.Type)
                {
                    case VirtualPath.PathType.AdToPwm:
                        for (int srcCh = _VirtualPath.SourceStartId; srcCh <= _VirtualPath.SourceEndId; srcCh++)
                        {
                            for (int dstCh = _VirtualPath.DstStartId; dstCh <= _VirtualPath.DstEndId; dstCh++)
                            {
                                int idx = dstCh * PublicConfig.AdChNum + srcCh;

                                ioState.UserDataToEcuStructure1.FromAdToPwmGain[idx] = _VirtualPath.Gain;
                                ioState.UserDataToEcuStructure1.FromAdToPwmDelaySmp[idx] = _VirtualPath.Delay;
                            }
                        }
                        break;
                    case VirtualPath.PathType.AdToSpiout:
                        for (int srcCh = _VirtualPath.SourceStartId; srcCh <= _VirtualPath.SourceEndId; srcCh++)
                        {
                            for (int dstCh = _VirtualPath.DstStartId; dstCh <= _VirtualPath.DstEndId; dstCh++)
                            {
                                int idx = dstCh * PublicConfig.AdChNum + srcCh;

                                ioState.UserDataToEcuStructure1.FromAdToSpioutGain[idx] = _VirtualPath.Gain;
                                ioState.UserDataToEcuStructure1.FromAdToSpioutDelaySmp[idx] = _VirtualPath.Delay;
                            }
                        }
                        break;
                    case VirtualPath.PathType.SpioutToPwm:
                    default:
                        for (int srcCh = _VirtualPath.SourceStartId; srcCh <= _VirtualPath.SourceEndId; srcCh++)
                        {
                            for (int dstCh = _VirtualPath.DstStartId; dstCh <= _VirtualPath.DstEndId; dstCh++)
                            {
                                int idx = dstCh * PublicConfig.SpioutChNum + srcCh;

                                ioState.UserDataToEcuStructure1.FromSpioutToPwmGain[idx] = _VirtualPath.Gain;
                                ioState.UserDataToEcuStructure1.FromSpioutToPwmDelaySmp[idx] = _VirtualPath.Delay;
                            }
                        }
                        break;
                }
                ioState.UserDataToEcuStructure1Update = true;
            }
        }

        private void ExecSquareWaveSetting(ControllerState ioState)
        {
            if (_SquareWave != null)
            {
                ioState.SquareWaveNumeratorCycle = _SquareWave.NumeratorCycle;
                ioState.SquareWaveDenominatorCycle = _SquareWave.DenominatorCycle;
            }
        }

        private void ExecSetVarSetting(ControllerState ioState)
        {
            if (_SetVar != null)
            {
                if (_IsNumeric)
                {
                    ioState.VariableDict[_SetVar.VarName] = _Value;
                }
                else
                {
                    ioState.VariableFormulaDict[_SetVar.VarName] = _SetVar.Value;
                }
            }
        }
    }
}
