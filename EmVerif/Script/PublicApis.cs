using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmVerif.Script.Command;

namespace EmVerif.Script
{
    public class PublicApis
    {
        public class Signal
        {
            public Byte Ch { get; set; }
        }

        public class FixWave : Signal
        {
            public Byte Id { get; set; }
            public float Freq { get; set; }
            public float Phase { get; set; }
            public float Gain { get; set; }

            public FixWave()
            {
                Ch = 0;
                Id = 0;
                Freq = 0;
                Phase = 0;
                Gain = 0;
            }
        }

        public class WhiteNoise : Signal
        {
            public float Gain { get; set; }

            public WhiteNoise()
            {
                Ch = 0;
                Gain = 0;
            }
        }

        public class RefWave : Signal
        {
            public Byte Id { get; set; }
            public string Freq { get; set; }
            public string Gain { get; set; }
            public string Phase { get; set; }

            public RefWave()
            {
                Ch = 0;
                Id = 0;
                Freq = "";
                Gain = "";
                Phase = "";
            }
        }

        public class VirtualPath
        {
            public string Path { get; set; }
            public float Gain { get; set; }
            public Byte Delay { get; set; }

            public VirtualPath()
            {
                Path = "In0_MixOut0";
                Gain = 0;
                Delay = 0;
            }
        }

        public class SetVar
        {
            public string VarName { get; set; }
            public double Value { get; set; }

            public SetVar()
            {
                VarName = "";
                Value = 0;
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
            SetVar SetVar = null,
            string Next = null
        )
        {
            SetCommand cmd = new SetCommand(inSignal: Signal, inVirtualPath: VirtualPath, inSetVar: SetVar, inNextState: Next);

            PublicController.Instance.Register(Trig, cmd);
        }

        public void GetWave(
            string Trig, string Stop,
            string FileName,
            string InId = null,
            string ThroughOutId = null,
            string MixOutId = null
        )
        {
            GetWaveCommand cmd = new GetWaveCommand(
                inStop: Stop,
                inFileName: FileName,
                inInId: InId,
                inThroughOutId: ThroughOutId,
                inMixOutId: MixOutId
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
            UInt32 ResponseCanId = 0xFFFFFFFF,
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
    }
}
