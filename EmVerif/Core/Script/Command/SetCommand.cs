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
        private enum MixSourceType
        {
            In,
            ThroughOut,
        }

        private class VirtualPath
        {
            public MixSourceType MixSourceType;
            public Int32 SourceStartId;
            public Int32 SourceEndId;
            public Int32 DstStartId;
            public Int32 DstEndId;
            public float Gain;
            public Byte Delay;

            private const string _SrcInRegexPattern = @"In(?<SrcId>\d+|\*)_MixOut(?<DstId>\d+|\*)";
            private const string _SrcThroughOutRegexPattern = @"ThroughOut(?<SrcId>\d+|\*)_MixOut(?<DstId>\d+|\*)";

            public VirtualPath(PublicApis.VirtualPath inVirtualPath)
            {
                var srcInRegex = new Regex(_SrcInRegexPattern);
                var srcThroughOutRegex = new Regex(_SrcThroughOutRegexPattern);
                var srcInMatch = srcInRegex.Match(inVirtualPath.Path);
                var srcThroughOutMatch = srcThroughOutRegex.Match(inVirtualPath.Path);

                Gain = inVirtualPath.Gain;
                Delay = inVirtualPath.Delay;
                if (srcInMatch.Success)
                {
                    MixSourceType = MixSourceType.In;
                    if ((string)srcInMatch.Groups["SrcId"].Value == @"*")
                    {
                        SourceStartId = 0;
                        SourceEndId = PublicConfig.InChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(srcInMatch.Groups["SrcId"].Value);

                        if (id < PublicConfig.InChNum)
                        {
                            SourceStartId = id;
                            SourceEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "In の設定範囲は、0 以上 " + PublicConfig.InChNum.ToString() + " 未満。"
                            );
                        }
                    }
                    if ((string)srcInMatch.Groups["DstId"].Value == @"*")
                    {
                        DstStartId = 0;
                        DstEndId = PublicConfig.MixOutChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(srcInMatch.Groups["DstId"].Value);

                        if (id < PublicConfig.MixOutChNum)
                        {
                            DstStartId = id;
                            DstEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "MixOut の設定範囲は、0 以上 " + PublicConfig.MixOutChNum.ToString() + " 未満。"
                            );
                        }
                    }
                }
                else if (srcThroughOutMatch.Success)
                {
                    MixSourceType = MixSourceType.ThroughOut;
                    if ((string)srcThroughOutMatch.Groups["SrcId"].Value == @"*")
                    {
                        SourceStartId = 0;
                        SourceEndId = PublicConfig.ThroughOutChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(srcThroughOutMatch.Groups["SrcId"].Value);

                        if (id < PublicConfig.ThroughOutChNum)
                        {
                            SourceStartId = id;
                            SourceEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "ThroughOut の設定範囲は、0 以上 " + PublicConfig.ThroughOutChNum.ToString() + " 未満。"
                            );
                        }
                    }
                    if ((string)srcThroughOutMatch.Groups["DstId"].Value == @"*")
                    {
                        DstStartId = 0;
                        DstEndId = PublicConfig.MixOutChNum - 1;
                    }
                    else
                    {
                        var id = Convert.ToInt32(srcThroughOutMatch.Groups["DstId"].Value);

                        if (id < PublicConfig.MixOutChNum)
                        {
                            DstStartId = id;
                            DstEndId = id;
                        }
                        else
                        {
                            throw new Exception(
                                "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                                "MixOut の設定範囲は、0 以上 " + PublicConfig.MixOutChNum.ToString() + " 未満。"
                            );
                        }
                    }
                }
                else
                {
                    throw new Exception(
                        "Set コマンド内 VirtualPath 設定エラー⇒" + inVirtualPath.Path + "\n" +
                        "In0_MixOut0、ThroughOut*_MixOut2 というように記述する。"
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

        public void Finally()
        {
        }

        private void CheckSignalParam()
        {
            if (_Signal != null)
            {
                if (_Signal.Ch >= PublicConfig.ThroughOutChNum)
                {
                    throw new Exception(
                        "Set コマンド内 Signal.Ch 設定エラー⇒" + _Signal.Ch + "\n" +
                        "Ch 設定値範囲は 0 以上 " + PublicConfig.ThroughOutChNum + " 未満。"
                    );
                }
                switch (_Signal.GetType().Name)
                {
                    case "SineWave":
                        {
                            PublicApis.SineWave signal = (PublicApis.SineWave)_Signal;
                            if (signal.Id >= PublicConfig.SignalBaseNum)
                            {
                                throw new Exception(
                                    "Set コマンド内 Signal.Id 設定エラー⇒" + signal.Id + "\n" +
                                    "Id 設定値範囲は 0 以上 " + PublicConfig.SignalBaseNum + " 未満。"
                                );
                            }
                        }
                        break;
                    case "WhiteNoise":
                        break;
                    default:
                        throw new Exception(
                            "不明な信号タイプ⇒" + _Signal.GetType().Name + "\n" +
                            "使用可能なクラスは、SineWave/WhiteNoise。"
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
                    case "SineWave":
                        {
                            PublicApis.SineWave signal = (PublicApis.SineWave)_Signal;
                            int idx = PublicConfig.SignalBaseNum * signal.Ch + signal.Id;

                            ioState.SineHzRef[idx] = signal.Freq;
                            ioState.SineGainRef[idx] = signal.Gain;
                            ioState.SinePhaseRef[idx] = signal.Phase;
                        }
                        break;
                    case "WhiteNoise":
                        {
                            PublicApis.WhiteNoise signal = (PublicApis.WhiteNoise)_Signal;
                            int idx = signal.Ch;

                            ioState.WhiteNoiseGainRef[idx] = signal.Gain;
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
                switch (_VirtualPath.MixSourceType)
                {
                    case MixSourceType.In:
                        for (int srcCh = _VirtualPath.SourceStartId; srcCh <= _VirtualPath.SourceEndId; srcCh++)
                        {
                            for (int dstCh = _VirtualPath.DstStartId; dstCh <= _VirtualPath.DstEndId; dstCh++)
                            {
                                int idx = dstCh * PublicConfig.InChNum + srcCh;

                                ioState.UserDataToEcuStructure.FromInToMixOutGain[idx] = _VirtualPath.Gain;
                                ioState.UserDataToEcuStructure.FromInToMixOutDelaySmp[idx] = _VirtualPath.Delay;
                            }
                        }
                        break;
                    case MixSourceType.ThroughOut:
                    default:
                        for (int srcCh = _VirtualPath.SourceStartId; srcCh <= _VirtualPath.SourceEndId; srcCh++)
                        {
                            for (int dstCh = _VirtualPath.DstStartId; dstCh <= _VirtualPath.DstEndId; dstCh++)
                            {
                                int idx = dstCh * PublicConfig.InChNum + srcCh;

                                ioState.UserDataToEcuStructure.FromThroughOutToMixOutGain[idx] = _VirtualPath.Gain;
                                ioState.UserDataToEcuStructure.FromThroughOutToMixOutDelaySmp[idx] = _VirtualPath.Delay;
                            }
                        }
                        break;
                }
            }
        }

        private void ExecSquareWaveSetting(ControllerState ioState)
        {
            if (_SquareWave != null)
            {
                ioState.SquareWaveNumeratorCycleRef = _SquareWave.NumeratorCycle;
                ioState.SquareWaveDenominatorCycleRef = _SquareWave.DenominatorCycle;
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
