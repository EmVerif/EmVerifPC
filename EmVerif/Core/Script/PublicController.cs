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
using EmVerif.Core.Communication;
using EmVerif.Core.Gui;
using EmVerif.Core.Script.Command;

namespace EmVerif.Core.Script
{
    public class PublicController
    {
        static public PublicController Instance = new PublicController();
        public event EventHandler EndEvent;

        private Dictionary<string, List<IEmVerifCommand>> _RegistrationListDict = new Dictionary<string, List<IEmVerifCommand>>();
        private List<IEmVerifCommand> _ExecList = new List<IEmVerifCommand>();
        private ControllerState _State = new ControllerState();

        private GuiTop _GuiTop = null;
        private Task _ExecScript10MsTask;
        private UInt32 _TaskTickCounterMs;
        private double _MaxLoad;
        private double _CurLoad;

        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_]\w*)");

        public IEnumerable<IPAddress> GetIpV4List()
        {
            return PublicCmd.Instance.GetIpV4List();
        }

        public void Reset()
        {
            _RegistrationListDict = new Dictionary<string, List<IEmVerifCommand>>();
            _ExecList = new List<IEmVerifCommand>();
            _State = new ControllerState();
            _RegistrationListDict.Add(ControllerState.BootStr, new List<IEmVerifCommand>());
            _RegistrationListDict.Add(ControllerState.EndStr, new List<IEmVerifCommand>());
        }

        public void Register(string inTrigger, IEmVerifCommand inCmd)
        {
            if (_RegistrationListDict.ContainsKey(inTrigger))
            {
                _RegistrationListDict[inTrigger].Add(inCmd);
            }
            else
            {
                List<IEmVerifCommand> list = new List<IEmVerifCommand>() { inCmd };
                _RegistrationListDict.Add(inTrigger, list);
            }
        }

        public void StartScript(IPAddress inIpAddress)
        {
            List<IEmVerifCommand> bootCmdList = _RegistrationListDict[ControllerState.BootStr];

            _GuiTop = new GuiTop();
            _GuiTop.Show();
            try
            {
                UInt16 tmp;

                PublicCmd.Instance.Start(inIpAddress);
                PublicCmd.Instance.SetCan(500, out tmp);
                PublicCmd.Instance.SetSpi(4, 4000, true, out tmp);
                PublicCmd.Instance.UploadProg("EmVerifCtrl.srec");
                PublicCmd.Instance.ProgExec();
            }
            catch (Exception ex)
            {
                PublicCmd.Instance.End();
                _GuiTop.Close();
                _GuiTop.Dispose();
                _GuiTop = null;
                throw ex;
            }
            LogManager.Instance.Start(@".\log.txt", _GuiTop);

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
            LogManager.Instance.Stop();
            if (_GuiTop != null)
            {
                _GuiTop.Close();
                _GuiTop.Dispose();
                _GuiTop = null;
            }
        }

        private void BootExecScript1MsTask(object sender, EventArgs e)
        {
            _TaskTickCounterMs++;
            if (
                (_TaskTickCounterMs >= PublicConfig.SamplingTimeMSec) &&
                (_ExecScript10MsTask.Status == TaskStatus.Created)
            )
            {
                _ExecScript10MsTask.Start();
                _TaskTickCounterMs -= PublicConfig.SamplingTimeMSec;
            }
        }

        private void ExecScript10MsTask()
        {
            try
            {
                _State.UserDataFromEcuStructureList = GetUserDataFromEcu();
                SetTimestamp();


                SetGraph();
                SetVariable();
                SetCanLog();
                _GuiTop.SetLoadBar(new List<double>() { _MaxLoad, _CurLoad });

                PreProcess();

                ExecCmd();
                ConvertExpression();
                PostProcess();
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
                _MaxLoad = Math.Max(_MaxLoad, (double)userDataFromEcu.MaxLoad10k * 100 / 3333);

                GCHandle gch = GCHandle.Alloc(userDataFromEcu.UserData, GCHandleType.Pinned);
                var userDataFromEcuStructure = (UserDataFromEcuStructure)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(UserDataFromEcuStructure));
                gch.Free();
                userDataFromEcuStructureList.Add(userDataFromEcuStructure);
            }

            return userDataFromEcuStructureList;
        }

        private void SetTimestamp()
        {
            if (_State.UserDataFromEcuStructureList.Count != 0)
            {
                _State.TimestampMs = _State.UserDataFromEcuStructureList.Last().Timestamp;
            }
        }

        private void PreProcess()
        {
            if (_State.CurrentState != _State.NextState)
            {
                if (_RegistrationListDict.ContainsKey(_State.NextState))
                {
                    List<IEmVerifCommand> bootCmdList = _RegistrationListDict[_State.NextState];
                    bootCmdList.ForEach((cmd) => cmd.Boot(_State));
                    _ExecList = _ExecList.Union(bootCmdList).ToList();
                }
                _State.CurrentState = _State.NextState;
                LogManager.Instance.Set(_State.TimestampMs.ToString("D10") + " State: " + _State.CurrentState + " Start");
                _GuiTop.SetState(_State.CurrentState);
            }
        }

        private void SetGraph()
        {
            List<double> currentInDataList = new List<double>();
            List<double> currentMixOutDataList = new List<double>();
            List<double> currentThroughOutDataList = new List<double>();
            foreach (var userDataFromEcuStructure in _State.UserDataFromEcuStructureList)
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

        private void SetVariable()
        {
            var updatedValueDict = _GuiTop.GetUpdatedValue();

            foreach (var varName in updatedValueDict.Keys)
            {
                _State.VariableDict[varName] = updatedValueDict[varName];
            }
            _GuiTop.SetVariable(_State.VariableDict, _State.VariableFormulaDict);
        }

        private void SetCanLog()
        {
            foreach (var userDataFromEcuStructure in _State.UserDataFromEcuStructureList)
            {
                string header = userDataFromEcuStructure.Timestamp.ToString("D10") + " ";

                for (int idx = 0; idx < userDataFromEcuStructure.CanRecvNum; idx++)
                {
                    string recvLog = header + "CAN Recv: ";

                    recvLog += "0x" + userDataFromEcuStructure.CanRecvData[idx].CanId.ToString("X3");
                    for (int byteNo = 0; byteNo < userDataFromEcuStructure.CanRecvData[idx].DataLen; byteNo++)
                    {
                        recvLog += " " + userDataFromEcuStructure.CanRecvData[idx].Data[byteNo].ToString("X2");
                    }
                    LogManager.Instance.Set(recvLog);
                }
                for (int idx = 0; idx < userDataFromEcuStructure.CanSendFinNum; idx++)
                {
                    string sendLog = header + "CAN Send: ";

                    sendLog += "0x" + userDataFromEcuStructure.CanSendFinData[idx].CanId.ToString("X3");
                    for (int byteNo = 0; byteNo < userDataFromEcuStructure.CanSendFinData[idx].DataLen; byteNo++)
                    {
                        sendLog += " " + userDataFromEcuStructure.CanSendFinData[idx].Data[byteNo].ToString("X2");
                    }
                    LogManager.Instance.Set(sendLog);
                }
            }
        }

        private void ExecCmd()
        {
            string nextState = _State.CurrentState;

            List<IEmVerifCommand> copyList = new List<IEmVerifCommand>(_ExecList);
            foreach (IEmVerifCommand cmd in copyList)
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
            _State.NextState = nextState;
        }

        private void ConvertExpression()
        {
            ConvertFormulaVar();
            ConvertSignal();
            ConvertSquareWave();
        }

        private void ConvertFormulaVar()
        {
            foreach (var varName in _State.VariableFormulaDict.Keys)
            {
                _State.VariableDict[varName] = (Decimal)ConvertFormula(_State.VariableFormulaDict[varName]);
            }
        }

        private void ConvertSignal()
        {
            for (int idx = 0; idx < (PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum); idx++)
            {
                if (_State.SineGainRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SineGain[idx] = (float)ConvertFormula(_State.SineGainRef[idx]);
                }
                if (_State.SineHzRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SineHz[idx] = (float)ConvertFormula(_State.SineHzRef[idx]);
                }
                if (_State.SinePhaseRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.SinePhase[idx] = (float)ConvertFormula(_State.SinePhaseRef[idx]);
                }
            }
            for (int idx = 0; idx < PublicConfig.ThroughOutChNum; idx++)
            {
                if (_State.WhiteNoiseGainRef[idx] != null)
                {
                    _State.UserDataToEcuStructure.WhiteNoiseGain[idx] = (float)ConvertFormula(_State.WhiteNoiseGainRef[idx]);
                }
            }
        }

        private void ConvertSquareWave()
        {
            if (_State.SquareWaveDenominatorCycleRef != null)
            {
                double val = ConvertFormula(_State.SquareWaveDenominatorCycleRef);

                val = Math.Max(Math.Min(65535.0, val), 0);
                _State.UserDataToEcuStructure.SquareWaveDenominatorCycle = (UInt16)val;
            }
            if (_State.SquareWaveNumeratorCycleRef != null)
            {
                double val = ConvertFormula(_State.SquareWaveNumeratorCycleRef);

                val = Math.Max(Math.Min(65535.0, val), 0);
                _State.UserDataToEcuStructure.SquareWaveNumeratorCycle = (UInt16)val;
            }
        }

        private double ConvertFormula(string inOrgFormula)
        {
            DataTable dt = new DataTable();
            var varNameMatches = _VarNameRegex.Matches(inOrgFormula);
            string resultStr = inOrgFormula;

            if (varNameMatches.Count != 0)
            {
                foreach (Match varNameMatch in varNameMatches)
                {
                    string varName = (string)varNameMatch.Groups["VarName"].Value;

                    try
                    {
                        resultStr = resultStr.Replace(varName, _State.VariableDict[varName].ToString());
                    }
                    catch
                    {
                        throw new Exception("不明な文字列⇒" + varName);
                    }
                }
            }

            return Convert.ToDouble(dt.Compute(resultStr, ""));
        }

        private void PostProcess()
        {
            int size = Marshal.SizeOf(_State.UserDataToEcuStructure);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_State.UserDataToEcuStructure, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            PublicCmd.Instance.SetUserData(bytes.ToList());
            _State.UserDataToEcuStructure.CanSendNum = 0;
            if (
                (_State.CurrentState == ControllerState.EndStr) ||
                (_GuiTop.FormClosingRequest)
            )
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
            else if (
                (PublicCmd.Instance.PcRecvErrorCounter != 0) ||
                (PublicCmd.Instance.EcuRecvErrorCounter != 0)
            )
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    MessageBox.Show(
                        "通信障害発生\n" +
                        "受信エラー数：" + PublicCmd.Instance.PcRecvErrorCounter + "\n" +
                        "送信エラー数：" + PublicCmd.Instance.EcuRecvErrorCounter
                    );
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
            else if (!PublicCmd.Instance.EcuActive)
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    MessageBox.Show("Ecu 反応無し");
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
            else
            {
                _ExecScript10MsTask = new Task(ExecScript10MsTask);
            }
        }

        private void Finally()
        {
            List<IEmVerifCommand> finallyList = new List<IEmVerifCommand>();

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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanRecvData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanSendFinData;
        public UInt32 CanRecvNum;
        public UInt32 CanSendFinNum;
        public UInt32 CanSendPossibleNum;
        public UInt32 CError;
        public UInt32 CStatus;

        public UInt32 Timestamp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.InChNum)]
        public UInt16[] InVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.MixOutChNum)]
        public UInt16[] MixOutVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.ThroughOutChNum)]
        public UInt16[] ThroughOutVal;

        public UserDataFromEcuStructure()
        {
            CanRecvData = new CanFormat[EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum];
            CanSendFinData = new CanFormat[EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum];
            for (int i = 0; i < CanRecvData.Length; i++)
            {
                CanRecvData[i].Data = new Byte[8];
            }
            for (int i = 0; i < CanSendFinData.Length; i++)
            {
                CanSendFinData[i].Data = new Byte[8];
            }

            InVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.InChNum];
            MixOutVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.MixOutChNum];
            ThroughOutVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.ThroughOutChNum];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataToEcuStructure
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum)]
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
        public UInt16 SquareWaveNumeratorCycle;
        public UInt16 SquareWaveDenominatorCycle;

        public UserDataToEcuStructure()
        {
            CanSendData = new CanFormat[EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum];
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
            SquareWaveNumeratorCycle = UInt16.MaxValue / 2;
            SquareWaveDenominatorCycle = UInt16.MaxValue;
        }
    }

    public class ControllerState
    {
        public UserDataToEcuStructure UserDataToEcuStructure;
        public IReadOnlyList<UserDataFromEcuStructure> UserDataFromEcuStructureList;
        public string CurrentState;
        public string NextState;
        public UInt32 TimestampMs;
        public Dictionary<string, Decimal> VariableDict;
        public Dictionary<string, string> VariableFormulaDict;
        public IReadOnlyList<double> CurrentInDataList;
        public IReadOnlyList<double> CurrentMixOutDataList;
        public IReadOnlyList<double> CurrentThroughOutDataList;
        public string[] SineHzRef;
        public string[] SineGainRef;
        public string[] SinePhaseRef;
        public string[] WhiteNoiseGainRef;
        public string SquareWaveNumeratorCycleRef;
        public string SquareWaveDenominatorCycleRef;

        public const string BootStr = "Boot";
        public const string EndStr = "End";

        public ControllerState()
        {
            UserDataToEcuStructure = new UserDataToEcuStructure();
            CurrentState = "";
            NextState = BootStr;
            TimestampMs = 0;
            VariableDict = new Dictionary<string, Decimal>();
            VariableFormulaDict = new Dictionary<string, string>();
            CurrentInDataList = new List<double>();
            CurrentMixOutDataList = new List<double>();
            CurrentThroughOutDataList = new List<double>();
            SineHzRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SineGainRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            SinePhaseRef = new string[PublicConfig.ThroughOutChNum * PublicConfig.SignalBaseNum];
            WhiteNoiseGainRef = new string[PublicConfig.ThroughOutChNum];
        }
    }
}
