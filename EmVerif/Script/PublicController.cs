using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EmVerif.Communication;
using EmVerif.Gui;
using EmVerif.Script.Command;

namespace EmVerif.Script
{
    public class PublicController
    {
        static public PublicController Instance = new PublicController();
        public const UInt32 SamplingTimeMSec = 10;
        public event EventHandler EndEvent;

        private Dictionary<string, List<ICommand>> _RegistrationListDict = new Dictionary<string, List<ICommand>>();
        private List<ICommand> _ExecList = new List<ICommand>();
        private ControllerState _State = new ControllerState();

        private GuiTop _GuiTop;
        private Task _ExecScript10MsTask;
        private UInt32 _TaskTickCounterMs;
        private double _MaxLoad;
        private double _CurLoad;

        public IEnumerable<IPAddress> GetIpV4List()
        {
            return PublicCmd.Instance.GetIpV4List();
        }

        public void Reset()
        {
            _RegistrationListDict = new Dictionary<string, List<ICommand>>();
            _ExecList = new List<ICommand>();
            _State = new ControllerState();
            _RegistrationListDict.Add(ControllerState.BootStr, new List<ICommand>());
            _RegistrationListDict.Add(ControllerState.EndStr, new List<ICommand>());
        }

        public void Register(string inTrigger, ICommand inCmd)
        {
            if (_RegistrationListDict.ContainsKey(inTrigger))
            {
                _RegistrationListDict[inTrigger].Add(inCmd);
            }
            else
            {
                List<ICommand> list = new List<ICommand>() { inCmd };
                _RegistrationListDict.Add(inTrigger, list);
            }
        }

        public void StartScript(IPAddress inIpAddress)
        {
            List<ICommand> bootCmdList = _RegistrationListDict[ControllerState.BootStr];

            _GuiTop = new GuiTop();
            _GuiTop.Show();
            try
            {
                UInt16 tmp;
                IReadOnlyList<UInt16> tmpList;

                PublicCmd.Instance.Start(inIpAddress);
                PublicCmd.Instance.SetCan(500, out tmp);
                PublicCmd.Instance.SetSpi(new List<Byte>() { 2, 1 }, new List<UInt16>() { 5000, 300 }, new List<bool>() { true, true }, out tmpList);
                PublicCmd.Instance.UploadProg("EmVerifCtrl.srec");
                PublicCmd.Instance.ProgExec();
            }
            catch (Exception ex)
            {
                PublicCmd.Instance.End();
                _GuiTop.Close();
                _GuiTop.Dispose();
                throw ex;
            }
            bootCmdList.ForEach((cmd) => cmd.Boot(_State));
            _ExecList = _ExecList.Union(bootCmdList).ToList();
            _ExecScript10MsTask = new Task(ExecScript10MsTask);

            _TaskTickCounterMs = 0;
            _CurLoad = 0;
            _MaxLoad = 0;
            PublicCmd.Instance.OnTimer += BootExecScript1MsTask;
        }

        public void StopScript()
        {
            PublicCmd.Instance.OnTimer -= BootExecScript1MsTask;
            PublicCmd.Instance.End();
            Finally();
            _GuiTop.Close();
            _GuiTop.Dispose();
        }

        private void BootExecScript1MsTask(object sender, EventArgs e)
        {
            _TaskTickCounterMs++;
            if (
                (_TaskTickCounterMs >= SamplingTimeMSec) &&
                (_ExecScript10MsTask.Status == TaskStatus.Created)
            )
            {
                _ExecScript10MsTask.Start();
                _TaskTickCounterMs -= SamplingTimeMSec;
            }
        }

        private void ExecScript10MsTask()
        {
            try
            {
                string nextState;

                _State.UserDataFromEcuStructureList = GetUserDataFromEcu();
                SetTimestamp(_State.UserDataFromEcuStructureList);
                SetGraph(_State.UserDataFromEcuStructureList);
                SetLoadBar(_State.UserDataFromEcuStructureList);
                nextState = ExecCmd();
                ConvertVariable();
                PostProcess(nextState);

                _State.CurrentState = nextState;
            }
            catch (Exception ex)
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
        }

        private IReadOnlyList<UserDataFromEcuStructure> GetUserDataFromEcu()
        {
            IReadOnlyList<UserDataFromEcu> userDataFromEcuList = PublicCmd.Instance.GetUserData();
            List<UserDataFromEcuStructure> userDataFromEcuStructureList = new List<UserDataFromEcuStructure>();

            foreach (var userDataFromEcu in userDataFromEcuList)
            {
                _CurLoad = userDataFromEcu.CurLoad1k * 100 / 33330;
                _MaxLoad = Math.Max(_MaxLoad, (double)userDataFromEcu.MaxLoad1k * 100 / 33330);
                _MaxLoad = Math.Max(_MaxLoad, (double)userDataFromEcu.MaxLoad5k * 100 / 6666);

                GCHandle gch = GCHandle.Alloc(userDataFromEcu.UserData, GCHandleType.Pinned);
                var userDataFromEcuStructure = (UserDataFromEcuStructure)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(UserDataFromEcuStructure));
                gch.Free();
                userDataFromEcuStructureList.Add(userDataFromEcuStructure);
            }

            return userDataFromEcuStructureList;
        }

        private void SetTimestamp(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList)
        {
            if (inUserDataFromEcuStructureList.Count != 0)
            {
                _State.TimestampMs = inUserDataFromEcuStructureList.Last().Timestamp;
            }
        }

        private void SetGraph(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList)
        {
            List<double> currentInDataList = new List<double>();
            List<double> currentMixOutDataList = new List<double>();
            List<double> currentThroughOutDataList = new List<double>();
            foreach (var userDataFromEcuStructure in inUserDataFromEcuStructureList)
            {
                foreach (var data in userDataFromEcuStructure.InVal)
                {
                    currentInDataList.Add(((double)data - 32768) / 32768);
                }
                foreach (var data in userDataFromEcuStructure.MixOutVal)
                {
                    currentMixOutDataList.Add(((double)data - 32768) / 32768);
                }
                foreach (var data in userDataFromEcuStructure.ThroughOutVal)
                {
                    currentThroughOutDataList.Add(((double)data - 32768) / 32768);
                }
            }
            _State.CurrentInDataList = currentInDataList;
            _State.CurrentMixOutDataList = currentMixOutDataList;
            _State.CurrentThroughOutDataList = currentThroughOutDataList;
            _GuiTop.SetGraph(currentInDataList, currentMixOutDataList, currentThroughOutDataList);
        }

        private void SetLoadBar(IReadOnlyList<UserDataFromEcuStructure> inUserDataFromEcuStructureList)
        {
            _GuiTop.SetLoadBar(new List<double>() { _MaxLoad, _CurLoad });
        }

        private string ExecCmd()
        {
            string nextState = _State.CurrentState;

            List<ICommand> copyList = new List<ICommand>(_ExecList);
            foreach (ICommand cmd in copyList)
            {
                Boolean finFlag;
                string state = cmd.ExecPer10Ms(_State, out finFlag);

                if (finFlag)
                {
                    _ExecList.Remove(cmd);
                    if (state != null)
                    {
                        nextState = state;
                    }
                }
            }

            return nextState;
        }

        private void ConvertVariable()
        {
            var varNameRegex = new Regex(@"(?<VarName>[a-zA-Z_]\w*)");
            DataTable dt = new DataTable();

            for (int idx = 0; idx < (PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum); idx++)
            {
                if (_State.SineGainRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SineGain[idx] = (float)ConvertVariableCore(_State.SineGainRef[idx]);
                }
                if (_State.SineHzRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SineHz[idx] = (float)ConvertVariableCore(_State.SineHzRef[idx]);
                }
                if (_State.SinePhaseRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SinePhase[idx] = (float)ConvertVariableCore(_State.SinePhaseRef[idx]);
                }
            }
        }

        private double ConvertVariableCore(string inOrgFormula)
        {
            var varNameRegex = new Regex(@"(?<VarName>[a-zA-Z_]\w*)");
            DataTable dt = new DataTable();
            var varNameMatches = varNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    resultStr = resultStr.Replace(varName, _State.VariableDict[varName].ToString());
                }
            }
            return Convert.ToDouble(dt.Compute(resultStr, ""));

        }

        private void PostProcess(string inNextState)
        {
            int size = Marshal.SizeOf(_State.UserDataToEcuStructure);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_State.UserDataToEcuStructure, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            PublicCmd.Instance.SetUserData(bytes.ToList());
            _State.UserDataToEcuStructure.CanSendNum = 0;
            if (_State.CurrentState == ControllerState.EndStr)
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
            else
            {
                if ((_State.CurrentState != inNextState) && _RegistrationListDict.ContainsKey(inNextState))
                {
                    List<ICommand> bootCmdList = _RegistrationListDict[inNextState];
                    bootCmdList.ForEach((cmd) => cmd.Boot(_State));
                    _ExecList = _ExecList.Union(bootCmdList).ToList();
                }
                _ExecScript10MsTask = new Task(ExecScript10MsTask);
            }
        }

        private void Finally()
        {
            List<ICommand> finallyList = new List<ICommand>();

            _RegistrationListDict.Keys.ToList().ForEach(stateName => finallyList.AddRange(_RegistrationListDict[stateName]));
            finallyList.ForEach(cmd => cmd.Finally());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CanFormat
    {
        public UInt32 CanId;
        public Byte IsExtendedId;
        public Byte IsRemoteFrame;
        public Byte DataLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataFromEcuStructure
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanRecvData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanSendFinData;
        public UInt32 CanRecvNum;
        public UInt32 CanSendFinNum;
        public UInt32 CanSendPossibleNum;
        public UInt32 CError;
        public UInt32 CStatus;

        public UInt32 Timestamp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.InChNum)]
        public UInt16[] InVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.MixOutChNum)]
        public UInt16[] MixOutVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.ThroughOutChNum)]
        public UInt16[] ThroughOutVal;

        public UserDataFromEcuStructure()
        {
            CanRecvData = new CanFormat[EmVerif.Communication.PublicConfig.MaxCanFifoNum];
            CanSendFinData = new CanFormat[EmVerif.Communication.PublicConfig.MaxCanFifoNum];
            for (int i = 0; i < CanRecvData.Length; i++)
            {
                CanRecvData[i].Data = new Byte[8];
            }
            for (int i = 0; i < CanSendFinData.Length; i++)
            {
                CanSendFinData[i].Data = new Byte[8];
            }

            InVal = new UInt16[EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.InChNum];
            MixOutVal = new UInt16[EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.MixOutChNum];
            ThroughOutVal = new UInt16[EmVerif.Communication.PublicConfig.SamplingKhz * PublicConfig.ThroughOutChNum];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataToEcuStructure
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanSendData;
        public UInt32 CanSendNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum)]
        public float[] SineHz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum)]
        public float[] SineGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum)]
        public float[] SinePhase;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.ThroughOutChNum)]
        public float[] WhiteNoiseGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.MixOutChNum * PublicConfig.InChNum)]
        public float[] FromInToMixOutGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.MixOutChNum * PublicConfig.ThroughOutChNum)]
        public float[] FromThroughOutToMixOutGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.MixOutChNum * PublicConfig.InChNum)]
        public Byte[] FromInToMixOutDelaySmp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.MixOutChNum * PublicConfig.ThroughOutChNum)]
        public Byte[] FromThroughOutToMixOutDelaySmp;

        public UserDataToEcuStructure()
        {
            CanSendData = new CanFormat[EmVerif.Communication.PublicConfig.MaxCanFifoNum];
            for (int i = 0; i < CanSendData.Length; i++)
            {
                CanSendData[i].Data = new Byte[8];
            }

            SineHz = new float[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SineGain = new float[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SinePhase = new float[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            WhiteNoiseGain = new float[PublicConfig.ThroughOutChNum];
            FromInToMixOutGain = new float[PublicConfig.MixOutChNum * PublicConfig.InChNum];
            FromThroughOutToMixOutGain = new float[PublicConfig.MixOutChNum * PublicConfig.ThroughOutChNum];
            FromInToMixOutDelaySmp = new Byte[PublicConfig.MixOutChNum * PublicConfig.InChNum];
            FromThroughOutToMixOutDelaySmp = new Byte[PublicConfig.MixOutChNum * PublicConfig.ThroughOutChNum];
        }
    }

    public class ControllerState
    {
        public UserDataToEcuStructure UserDataToEcuStructure;
        public IReadOnlyList<UserDataFromEcuStructure> UserDataFromEcuStructureList;
        public string CurrentState;
        public UInt32 TimestampMs;
        public Dictionary<string, double> VariableDict;
        public IReadOnlyList<double> CurrentInDataList;
        public IReadOnlyList<double> CurrentMixOutDataList;
        public IReadOnlyList<double> CurrentThroughOutDataList;
        public string[] SineHzRef;
        public string[] SineGainRef;
        public string[] SinePhaseRef;

        public const string BootStr = "Boot";
        public const string EndStr = "End";

        public ControllerState()
        {
            UserDataToEcuStructure = new UserDataToEcuStructure();
            CurrentState = BootStr;
            TimestampMs = 0;
            VariableDict = new Dictionary<string, double>();
            CurrentInDataList = new List<double>();
            CurrentMixOutDataList = new List<double>();
            CurrentThroughOutDataList = new List<double>();
            SineHzRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SineGainRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SinePhaseRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
        }
    }
}
