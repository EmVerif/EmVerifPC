using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmVerif.Core.Script.Command;

namespace EmVerif.Core.Script
{
    public class PublicApis
    {
        public class Signal
        {
            public Byte Ch { get; set; }
            public string Gain { get; set; }
        }

        public class PwmWhiteNoise : Signal
        {
            public PwmWhiteNoise()
            {
                Ch = 0;
                Gain = "0";
            }
        }

        public class SpioutWhiteNoise : Signal
        {
            public SpioutWhiteNoise()
            {
                Ch = 0;
                Gain = "0";
            }
        }

        public class PwmSineWave : Signal
        {
            public Byte Id { get; set; }
            public string Freq { get; set; }
            public string Phase { get; set; }

            public PwmSineWave()
            {
                Ch = 0;
                Id = 0;
                Freq = "0";
                Gain = "0";
                Phase = "0";
            }
        }

        public class SpioutSineWave : Signal
        {
            public Byte Id { get; set; }
            public string Freq { get; set; }
            public string Phase { get; set; }

            public SpioutSineWave()
            {
                Ch = 0;
                Id = 0;
                Freq = "0";
                Gain = "0";
                Phase = "0";
            }
        }

        public class VirtualPath
        {
            public string Path { get; set; }
            public float Gain { get; set; }
            public Byte Delay { get; set; }

            public VirtualPath()
            {
                Path = "Ad0_Pwm0";
                Gain = 0;
                Delay = 0;
            }
        }

        public class SetVar
        {
            public string VarName { get; set; }
            public string Value { get; set; }

            public SetVar()
            {
                VarName = "";
                Value = "";
            }
        }

        public class SquareWave
        {
            public string NumeratorCycle { get; set; }
            public string DenominatorCycle { get; set; }

            public SquareWave()
            {
                NumeratorCycle = (UInt16.MaxValue / 2).ToString();
                DenominatorCycle = UInt16.MaxValue.ToString();
            }
        }

        public class CanDataMask
        {
            public UInt32 BytePos { get; set; }
            public UInt32 BitPos { get; set; }
            public UInt32 BitLen { get; set; }
            public string RefVar { get; set; }
        }

        public void Wait(
            string Trig, string Next,
            double Time
        )
        {
            WaitCommand cmd = new WaitCommand(inWaitTimeSec: Time, inNextState: Next);

            PublicController.Instance.Register(Trig, cmd);
        }

        public void Set(
            string Trig,
            Signal Signal = null,
            VirtualPath VirtualPath = null,
            SquareWave SquareWave = null,
            SetVar SetVar = null,
            string PortOut = null,
            string Next = null
        )
        {
            SetCommand cmd = new SetCommand(
                inSignal: Signal,
                inVirtualPath: VirtualPath,
                inSquareWave: SquareWave,
                inSetVar: SetVar,
                inPortOut: PortOut,
                inNextState: Next
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void GetWave(
            string Trig, string Stop,
            string FileName,
            string AdId = null,
            string SpioutId = null,
            string PwmId = null
        )
        {
            GetWaveCommand cmd = new GetWaveCommand(
                inStop: Stop,
                inFileName: FileName,
                inAdId: AdId,
                inSpioutId: SpioutId,
                inPwmId: PwmId
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void CanSend(
            string Trig,
            UInt32 SendCanId,
            List<Byte> Data,
            CanDataMask DataMask = null,
            List<CanDataMask> DataMaskList = null,
            UInt32 ResponseCanId = CanSendCommand.NoValue,
            string Next = null
        )
        {
            CanSendCommand cmd = new CanSendCommand(
                inSendCanId: SendCanId,
                inSendDataList: Data,
                inDataMask: DataMask,
                inDataMaskList: DataMaskList,
                inResponseCanId: ResponseCanId,
                inNextState: Next
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void CanRepSend(
            string Trig,
            UInt32 SendCanId,
            List<Byte> Data,
            double RepTime,
            CanDataMask DataMask = null,
            List<CanDataMask> DataMaskList = null,
            UInt32 ResponseCanId = CanSendCommand.NoValue,
            string Stop = null
        )
        {
            CanSendCommand cmd = new CanSendCommand(
                inSendCanId: SendCanId,
                inSendDataList: Data,
                inDataMask: DataMask,
                inDataMaskList: DataMaskList,
                inResponseCanId: ResponseCanId,
                inRepeatTime: RepTime,
                inStopState: Stop
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void CanDiagSend(
            string Trig,
            UInt32 SendCanId,
            List<Byte> SendData,
            UInt32 SendNta = CanDiagSendCommand.NoValue,
            UInt32 ResponseCanId = CanDiagSendCommand.NoValue,
            UInt32 ResponseNta = CanDiagSendCommand.NoValue,
            string ResponseDataArrayName = null,
            string Next = null
        )
        {
            CanDiagSendCommand cmd = new CanDiagSendCommand(
                inSendCanId: SendCanId,
                inSendNta: SendNta,
                inSendData: SendData,
                inResponseCanId: ResponseCanId,
                inResponseNta: ResponseNta,
                inResponseDataArrayName: ResponseDataArrayName,
                inNext: Next
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void SilenceCheck(
            string Trig, string Stop,
            string AdId,
            double Thresh = 0.01,
            string Message = null
        )
        {
            SilenceCheckCommand cmd = new SilenceCheckCommand(
                inStop: Stop,
                inAdId: AdId,
                inThresh: Thresh,
                inMessage: Message
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void ExecBatch(
            string Trig,
            string Cmd,
            string Next = null
        )
        {
            ExecBatchCommand cmd = new ExecBatchCommand(
                inCmd: Cmd,
                inNext: Next
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void DcCut(
            string Trig,
            Boolean OnOff
        )
        {
            DcCutCommand cmd = new DcCutCommand(
                inOnOff: OnOff
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void Fade(
            string Trig,
            string VarName,
            Decimal Target,
            double Time,
            string Next = null
        )
        {
            FadeCommand cmd = new FadeCommand(
                inVarName: VarName,
                inTarget: Target,
                inTime: Time,
                inNext: Next
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void CalcFft(
            string Trig, string Stop,
            List<double> InspectFreqList,
            List<double> InspectFreqDeltaList = null,
            Int32 FftNum = 32768,
            string AmpResultArrayName = null,
            string PhaseResultArrayName = null,
            string OtherAmpResultVarName = null,
            UInt32 AdId = CalcFftCommand.NoSelectValue,
            UInt32 SpioutId = CalcFftCommand.NoSelectValue,
            UInt32 PwmId = CalcFftCommand.NoSelectValue,
            string Message = null
        )
        {
            CalcFftCommand cmd = new CalcFftCommand(
                inStop: Stop,
                inInspectFreqList: InspectFreqList,
                inInspectFreqDeltaList: InspectFreqDeltaList,
                inFftNum: FftNum,
                inAmpResultArrayName: AmpResultArrayName,
                inPhaseResultArrayName: PhaseResultArrayName,
                inOtherAmpResultVarName: OtherAmpResultVarName,
                inAdId: AdId,
                inSpioutId: SpioutId,
                inPwmId: PwmId,
                inMessage: Message
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void CalcDft(
            string Trig, string Stop,
            List<double> InspectFreqList,
            string AmpResultArrayName = null,
            string PhaseResultArrayName = null,
            UInt32 AdId = CalcFftCommand.NoSelectValue,
            UInt32 SpioutId = CalcFftCommand.NoSelectValue,
            UInt32 PwmId = CalcFftCommand.NoSelectValue,
            string Message = null
        )
        {
            CalcDftCommand cmd = new CalcDftCommand(
                inStop: Stop,
                inInspectFreqList: InspectFreqList,
                inAmpResultArrayName: AmpResultArrayName,
                inPhaseResultArrayName: PhaseResultArrayName,
                inAdId: AdId,
                inSpioutId: SpioutId,
                inPwmId: PwmId,
                inMessage: Message
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void ValueCheck(
            string CheckValue,
            Decimal ExpValueMax,
            Decimal ExpValueMin,
            string Message = null
        )
        {
            ValueCheckCommand cmd = new ValueCheckCommand(
                inCheckValue: CheckValue,
                inExpValueMax: ExpValueMax,
                inExpValueMin: ExpValueMin,
                inMessage: Message
            );

            PublicController.Instance.Register("End", cmd);
        }
    }
}
