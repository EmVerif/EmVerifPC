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
            public string NumeratorCycle;
            public string DenominatorCycle;

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
            UInt32 ResponseCanId = CanSendCommand.NoResponseValue,
            string Next = null
        )
        {
            CanSendCommand cmd = new CanSendCommand(
                inSendCanId: SendCanId,
                inSendDataList: Data,
                inDataMask: DataMask,
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
            UInt32 ResponseCanId = CanSendCommand.NoResponseValue,
            string Stop = null
        )
        {
            CanSendCommand cmd = new CanSendCommand(
                inSendCanId: SendCanId,
                inSendDataList: Data,
                inDataMask: DataMask,
                inResponseCanId: ResponseCanId,
                inRepeatTime: RepTime,
                inStopState: Stop
            );

            PublicController.Instance.Register(Trig, cmd);
        }

        public void SilenceCheck(
            string Trig, string Stop,
            string AdId,
            double Thresh = 0.01,
            string Message = ""
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
    }
}
