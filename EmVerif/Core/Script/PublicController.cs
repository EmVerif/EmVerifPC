using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using EmVerif.Core.Gui.Variable;
using EmVerif.Core.Script.Command;

namespace EmVerif.Core.Script
{
    public class PublicController
    {
        static public PublicController Instance = new PublicController();
        public event EventHandler EndEvent;

        private Dictionary<string, List<IEmVerifCommand>> _RegistrationListDict;
        private List<IEmVerifCommand> _ExecList;
        private ControllerState _State;

        private GuiTop _GuiTop = null;
        private Task _ExecScript10MsTask;
        private UInt32 _TaskTickCounterMs;
        private double _MaxLoad;
        private double _CurLoad;

        private Regex _VarNameRegex = new Regex(@"(?<VarName>[a-zA-Z_][\w\[\]]*)");

        public IEnumerable<IPAddress> GetIpV4List()
        {
            return PublicCmd.Instance.GetIpV4List();
        }

        public void Reset(in string inWorkFolder)
        {
            _RegistrationListDict = new Dictionary<string, List<IEmVerifCommand>>();
            _ExecList = new List<IEmVerifCommand>();
            _State = new ControllerState(inWorkFolder);
            _RegistrationListDict.Add(ControllerState.BootStr, new List<IEmVerifCommand>());
            _RegistrationListDict.Add(ControllerState.EndStr, new List<IEmVerifCommand>());
        }

        public void Register(in string inTrigger, in IEmVerifCommand inCmd)
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

        public void StartScript(in IPAddress inIpAddress)
        {
            List<IEmVerifCommand> bootCmdList = _RegistrationListDict[ControllerState.BootStr];

            _GuiTop = new GuiTop();
            _GuiTop.Show();
            try
            {
                UInt16 tmp;

                Directory.CreateDirectory(_State.WorkFolder);
                LogManager.Instance.Start(_State.WorkFolder + @".\log.txt", _GuiTop);
                CanDiagProtocol.Instance.Initialize();
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
            if (
                (PublicCmd.Instance.PcRecvErrorCounter != 0) ||
                (PublicCmd.Instance.EcuRecvErrorCounter != 0)
            )
            {
                LogManager.Instance.Set("通信障害発生⇒NG");
                LogManager.Instance.Set("受信エラー数：" + PublicCmd.Instance.PcRecvErrorCounter);
                LogManager.Instance.Set("送信エラー数：" + PublicCmd.Instance.EcuRecvErrorCounter);
            }
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
                _CurLoad = Math.Max((double)userDataFromEcu.CurLoad1k * 100 / 33330, (double)userDataFromEcu.MaxLoad10k * 100 / 3333);
                _MaxLoad = Math.Max(_MaxLoad, _CurLoad);

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
            List<double> currentAdDataList = new List<double>();
            List<double> currentPwmDataList = new List<double>();
            List<double> currentSpioutDataList = new List<double>();
            foreach (var userDataFromEcuStructure in _State.UserDataFromEcuStructureList)
            {
                foreach (var data in userDataFromEcuStructure.AdVal)
                {
                    currentAdDataList.Add(((double)data - 32768) / 32768);
                }
                foreach (var data in userDataFromEcuStructure.PwmVal)
                {
                    currentPwmDataList.Add(((double)data - 32768) / 32768);
                }
                foreach (var data in userDataFromEcuStructure.SpioutVal)
                {
                    currentSpioutDataList.Add(((double)data - 32768) / 32768);
                }
            }
            _State.CurrentAdDataList = currentAdDataList;
            _State.CurrentPwmDataList = currentPwmDataList;
            _State.CurrentSpioutDataList = currentSpioutDataList;
            _GuiTop.SetGraph(currentAdDataList, currentPwmDataList, currentSpioutDataList);
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
            ConvertPortOut();
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
            for (int idx = 0; idx < (PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum); idx++)
            {
                if (_State.SpioutSineGain[idx] != null)
                {
                    _State.UserDataToEcuStructure0.SpioutSineGain[idx] = (float)ConvertFormula(_State.SpioutSineGain[idx]);
                }
                if (_State.SpioutSineHz[idx] != null)
                {
                    _State.UserDataToEcuStructure0.SpioutSineHz[idx] = (float)ConvertFormula(_State.SpioutSineHz[idx]);
                }
                if (_State.SpioutSinePhase[idx] != null)
                {
                    _State.UserDataToEcuStructure0.SpioutSinePhase[idx] = (float)ConvertFormula(_State.SpioutSinePhase[idx]);
                }
            }
            for (int idx = 0; idx < PublicConfig.SpioutChNum; idx++)
            {
                if (_State.SpioutWhiteNoiseGain[idx] != null)
                {
                    _State.UserDataToEcuStructure0.SpioutWhiteNoiseGain[idx] = (float)ConvertFormula(_State.SpioutWhiteNoiseGain[idx]);
                }
            }
            for (int idx = 0; idx < (PublicConfig.PwmChNum * PublicConfig.SignalBaseNum); idx++)
            {
                if (_State.PwmSineGain[idx] != null)
                {
                    _State.UserDataToEcuStructure0.PwmSineGain[idx] = (float)ConvertFormula(_State.PwmSineGain[idx]);
                }
                if (_State.PwmSineHz[idx] != null)
                {
                    _State.UserDataToEcuStructure0.PwmSineHz[idx] = (float)ConvertFormula(_State.PwmSineHz[idx]);
                }
                if (_State.PwmSinePhase[idx] != null)
                {
                    _State.UserDataToEcuStructure0.PwmSinePhase[idx] = (float)(ConvertFormula(_State.PwmSinePhase[idx]) / 180 * Math.PI);
                }
            }
            for (int idx = 0; idx < PublicConfig.PwmChNum; idx++)
            {
                if (_State.PwmWhiteNoiseGain[idx] != null)
                {
                    _State.UserDataToEcuStructure0.PwmWhiteNoiseGain[idx] = (float)ConvertFormula(_State.PwmWhiteNoiseGain[idx]);
                }
            }
        }

        private void ConvertSquareWave()
        {
            if (_State.SquareWaveDenominatorCycle != null)
            {
                double val = ConvertFormula(_State.SquareWaveDenominatorCycle);

                val = Math.Max(Math.Min(65535.0, val), 0);
                _State.UserDataToEcuStructure0.SquareWaveDenominatorCycle = (UInt16)val;
            }
            if (_State.SquareWaveNumeratorCycle != null)
            {
                double val = ConvertFormula(_State.SquareWaveNumeratorCycle);

                val = Math.Max(Math.Min(65535.0, val), 0);
                _State.UserDataToEcuStructure0.SquareWaveNumeratorCycle = (UInt16)val;
            }
        }

        private void ConvertPortOut()
        {
            if (_State.PortOut != null)
            {
                double val = ConvertFormula(_State.PortOut);

                val = Math.Max(Math.Min(255, val), 0);
                _State.UserDataToEcuStructure0.PortOut = (Byte)val;
            }
        }

        private double ConvertFormula(in string inOrgFormula)
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
            CanDiagProtocol.Instance.Process(
                _State.UserDataFromEcuStructureList,
                ref _State.UserDataToEcuStructure0,
                _State.TimestampMs
            );
            SendUserDataToEcuStructure0();
            SendUserDataToEcuStructure1();
            if (
                (_State.CurrentState == ControllerState.EndStr) ||
                (_GuiTop.FormClosingRequest) ||
                (PublicCmd.Instance.PcRecvErrorCounter != 0) ||
                (PublicCmd.Instance.EcuRecvErrorCounter != 0)
            )
            {
                _GuiTop.Invoke((Action)(() =>
                {
                    EndEvent?.Invoke(this, new EventArgs());
                }));
            }
            else
            {
                _ExecScript10MsTask = new Task(ExecScript10MsTask);
            }
        }

        private void SendUserDataToEcuStructure0()
        {
            int size = Marshal.SizeOf(_State.UserDataToEcuStructure0);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_State.UserDataToEcuStructure0, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            PublicCmd.Instance.SetUserData(bytes.ToList());
            _State.UserDataToEcuStructure0 = new UserDataToEcuStructure0();
        }

        private void SendUserDataToEcuStructure1()
        {
            if (_State.UserDataToEcuStructure1Update)
            {
                int size = Marshal.SizeOf(_State.UserDataToEcuStructure1);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(_State.UserDataToEcuStructure1, ptr, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                Marshal.FreeHGlobal(ptr);

                PublicCmd.Instance.SetUserData(bytes.ToList());
                _State.UserDataToEcuStructure1Update = false;
            }
        }

        private void Finally()
        {
            List<IEmVerifCommand> finallyList = new List<IEmVerifCommand>();

            foreach (var key in _RegistrationListDict.Keys.ToList())
            {
                if (key != ControllerState.EndStr)
                {
                    finallyList.AddRange(_RegistrationListDict[key]);
                }
            }
            finallyList.AddRange(_RegistrationListDict[ControllerState.EndStr]);
            foreach (var cmd in finallyList)
            {
                try
                {
                    cmd.Finally(_State);
                }
                catch
                {
                }
            }
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.AdChNum)]
        public UInt16[] AdVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.PwmChNum)]
        public UInt16[] PwmVal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.SpioutChNum)]
        public UInt16[] SpioutVal;

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

            AdVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.AdChNum];
            PwmVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.PwmChNum];
            SpioutVal = new UInt16[EmVerif.Core.Communication.PublicConfig.SamplingKhz * PublicConfig.SpioutChNum];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataToEcuStructure0
    {
        public Int32 Type = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum)]
        public CanFormat[] CanSendData;
        public UInt32 CanSendNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.SignalBaseNum)]
        public float[] PwmSineHz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.SignalBaseNum)]
        public float[] PwmSineGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.SignalBaseNum)]
        public float[] PwmSinePhase;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum)]
        public float[] PwmWhiteNoiseGain;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum)]
        public float[] SpioutSineHz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum)]
        public float[] SpioutSineGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum)]
        public float[] SpioutSinePhase;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum)]
        public float[] SpioutWhiteNoiseGain;

        public UInt16 SquareWaveNumeratorCycle;
        public UInt16 SquareWaveDenominatorCycle;

        public Byte PortOut = 0x00;

        public UserDataToEcuStructure0()
        {
            CanSendData = new CanFormat[EmVerif.Core.Communication.PublicConfig.MaxCanFifoNum];
            for (int i = 0; i < CanSendData.Length; i++)
            {
                CanSendData[i].Data = new Byte[8];
            }

            PwmSineHz = new float[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmSineGain = new float[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmSinePhase = new float[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmWhiteNoiseGain = new float[PublicConfig.PwmChNum];
            
            SpioutSineHz = new float[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutSineGain = new float[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutSinePhase = new float[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutWhiteNoiseGain = new float[PublicConfig.SpioutChNum];

            SquareWaveNumeratorCycle = UInt16.MaxValue / 2;
            SquareWaveDenominatorCycle = UInt16.MaxValue;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataToEcuStructure1
    {
        public Int32 Type = 1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.AdChNum)]
        public float[] FromAdToPwmGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.SpioutChNum)]
        public float[] FromSpioutToPwmGain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum * PublicConfig.AdChNum)]
        public float[] FromAdToSpioutGain;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.AdChNum)]
        public Byte[] FromAdToPwmDelaySmp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.PwmChNum * PublicConfig.SpioutChNum)]
        public Byte[] FromSpioutToPwmDelaySmp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.SpioutChNum * PublicConfig.AdChNum)]
        public Byte[] FromAdToSpioutDelaySmp;

        public Byte DcCutFlag = 0;

        public UserDataToEcuStructure1()
        {
            FromAdToPwmGain = new float[PublicConfig.PwmChNum * PublicConfig.AdChNum];
            FromSpioutToPwmGain = new float[PublicConfig.PwmChNum * PublicConfig.SpioutChNum];
            FromAdToSpioutGain = new float[PublicConfig.SpioutChNum * PublicConfig.AdChNum];

            FromAdToPwmDelaySmp = new Byte[PublicConfig.PwmChNum * PublicConfig.AdChNum];
            FromSpioutToPwmDelaySmp = new Byte[PublicConfig.PwmChNum * PublicConfig.SpioutChNum];
            FromAdToSpioutDelaySmp = new Byte[PublicConfig.SpioutChNum * PublicConfig.AdChNum];
        }
    }

    public class ControllerState
    {
        public string WorkFolder { get; private set; }
        public UserDataToEcuStructure0 UserDataToEcuStructure0;
        public bool UserDataToEcuStructure1Update;
        public UserDataToEcuStructure1 UserDataToEcuStructure1;
        public IReadOnlyList<UserDataFromEcuStructure> UserDataFromEcuStructureList;
        public string CurrentState;
        public string NextState;
        public UInt32 TimestampMs;
        public Dictionary<string, Decimal> VariableDict;
        public Dictionary<string, string> VariableFormulaDict;
        public IReadOnlyList<double> CurrentAdDataList;
        public IReadOnlyList<double> CurrentPwmDataList;
        public IReadOnlyList<double> CurrentSpioutDataList;
        public string[] SpioutSineHz;
        public string[] SpioutSineGain;
        public string[] SpioutSinePhase;
        public string[] SpioutWhiteNoiseGain;
        public string[] PwmSineHz;
        public string[] PwmSineGain;
        public string[] PwmSinePhase;
        public string[] PwmWhiteNoiseGain;
        public string SquareWaveNumeratorCycle;
        public string SquareWaveDenominatorCycle;
        public string PortOut;

        public const string BootStr = "Boot";
        public const string EndStr = "End";

        public ControllerState(in string inWorkFolder)
        {
            WorkFolder = inWorkFolder;
            UserDataToEcuStructure0 = new UserDataToEcuStructure0();
            UserDataToEcuStructure1Update = true;
            UserDataToEcuStructure1 = new UserDataToEcuStructure1();
            CurrentState = "";
            NextState = BootStr;
            TimestampMs = 0;
            VariableDict = new Dictionary<string, Decimal>();
            VariableFormulaDict = new Dictionary<string, string>();
            CurrentAdDataList = new List<double>();
            CurrentPwmDataList = new List<double>();
            CurrentSpioutDataList = new List<double>();

            SpioutSineHz = new string[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutSineGain = new string[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutSinePhase = new string[PublicConfig.SpioutChNum * PublicConfig.SignalBaseNum];
            SpioutWhiteNoiseGain = new string[PublicConfig.SpioutChNum];

            PwmSineHz = new string[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmSineGain = new string[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmSinePhase = new string[PublicConfig.PwmChNum * PublicConfig.SignalBaseNum];
            PwmWhiteNoiseGain = new string[PublicConfig.PwmChNum];
        }
    }
}
