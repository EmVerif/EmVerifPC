using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace EmVerif.Communication
{
    public class PublicCmd
    {
        public static PublicCmd Instance = new PublicCmd();
        public event EventHandler OnTimer
        {
            add
            {
                MmTimer.Instance.OnTimer += value;
            }
            remove
            {
                MmTimer.Instance.OnTimer -= value;
            }
        }
        public UInt32 PcRecvErrorCounter
        {
            get
            {
                return InternalCmd.Instance.PcRecvErrorCounter;
            }
        }
        public UInt32 EcuRecvErrorCounter
        {
            get
            {
                return InternalCmd.Instance.EcuRecvErrorCounter;
            }
        }
        public Boolean EcuActive
        {
            get
            {
                return InternalCmd.Instance.EcuActive;
            }
        }

        private Boolean[] _AckRecvFlag = new Boolean[Byte.MaxValue];
        private IReadOnlyList<byte>[] _RecvDataList = new List<byte>[Byte.MaxValue];
        private List<UserDataFromEcu> _UserDataList = new List<UserDataFromEcu>();
        private object _UserDataListLock = new object();

        public IEnumerable<IPAddress> GetIpV4List()
        {
            return InternalCmd.Instance.GetIpV4List();
        }

        public void Start(IPAddress inIpAddr)
        {
            _UserDataList = new List<UserDataFromEcu>();
            InternalCmd.Instance.SetRecvUserDataFromEcuEvent(RecvUserData);
            InternalCmd.Instance.Start(inIpAddr);
        }

        public void UploadProg(string inS3FileName)
        {
            UInt32 curAddr;
            UInt32 restSize;
            S3Analyzer.Instance.Read(inS3FileName);
            curAddr = S3Analyzer.Instance.GetTopAddr();
            restSize = S3Analyzer.Instance.GetSize();

            while(restSize > 0)
            {
                UInt32 curUploadSize = Math.Min(512, restSize);
                List<Byte> data = new List<Byte>();

                data.Add((Byte)((curAddr >> 24) & 0xFF));
                data.Add((Byte)((curAddr >> 16) & 0xFF));
                data.Add((Byte)((curAddr >> 8) & 0xFF));
                data.Add((Byte)(curAddr & 0xFF));
                data.AddRange(S3Analyzer.Instance.GetData(curAddr, curUploadSize));
                _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.UploadProgCmdId] = false;
                InternalCmd.Instance.ExecCmd(InternalCmd.PublicCmdId.UploadProgCmdId, data, RecvUploadProgCmdAck);
                WaitUploadProgCmdAck();

                restSize -= curUploadSize;
                curAddr += curUploadSize;
            }
        }

        public void ProgExec()
        {
            List<Byte> data = new List<byte>();

            _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.ProgExecCmdId] = false;
            InternalCmd.Instance.ExecCmd(InternalCmd.PublicCmdId.ProgExecCmdId, data, RecvProgExecCmdAck);
            WaitProgExecCmdAck();
        }

        public void SetUserData(IEnumerable<Byte> inData)
        {
            List<Byte> data = new List<byte>(inData);
            int count = data.Count;

            if (count > PublicConfig.UserByteNum)
            {
                data.RemoveRange(PublicConfig.UserByteNum, count - PublicConfig.UserByteNum);
            }
            data.InsertRange(0, new List<Byte>() { 0, 0 });
            InternalCmd.Instance.SendUserDataToEcu(data);
        }

        public IReadOnlyList<UserDataFromEcu> GetUserData()
        {
            IReadOnlyList<UserDataFromEcu> ret;

            lock (_UserDataListLock)
            {
                ret = _UserDataList;
                _UserDataList = new List<UserDataFromEcu>();
            }

            return ret;
        }

        public void SetCan(UInt16 inKbps, out UInt16 outKbps)
        {
            List<Byte> data = new List<byte>();

            outKbps = 0;
            data.Add((Byte)((inKbps >> 8) & 0xFF));
            data.Add((Byte)(inKbps & 0xFF));
            _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetCanCmdId] = false;
            InternalCmd.Instance.ExecCmd(InternalCmd.PublicCmdId.SetCanCmdId, data, RecvSetCanCmdAck);
            WaitSetCanCmdAck();
            outKbps = (UInt16)(
                (UInt16)_RecvDataList[(Byte)InternalCmd.PublicCmdId.SetCanCmdId][0] * 256 +
                (UInt16)_RecvDataList[(Byte)InternalCmd.PublicCmdId.SetCanCmdId][1]
            );
        }

        public void SetSpi(Byte inDataLen, UInt16 inKbps, Boolean inIs5khz, out UInt16 outKbps)
        {
            List<Byte> data = new List<byte>();

            data.Add((Byte)((inKbps >> 8) & 0xFF));
            data.Add((Byte)(inKbps & 0xFF));
            if (inIs5khz)
            {
                data.Add(1);
            }
            else
            {
                data.Add(0);
            }
            data.Add(inDataLen);
            _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId] = false;
            InternalCmd.Instance.ExecCmd(InternalCmd.PublicCmdId.SetSpiCmdId, data, RecvSetSpiCmdAck);
            WaitSetSpiCmdAck();
            outKbps = (UInt16)(
                (UInt16)_RecvDataList[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId][0] * 256 +
                (UInt16)_RecvDataList[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId][1]
            );
        }

        public void End()
        {
            InternalCmd.Instance.End();
        }

        #region プログラムアップロード処理
        private void RecvUploadProgCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            _RecvDataList[(Byte)InternalCmd.PublicCmdId.UploadProgCmdId]= inRecvDataList;
            _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.UploadProgCmdId] = true;
        }

        private void WaitUploadProgCmdAck()
        {
            int timeoutCounter = 10;

            while (!_AckRecvFlag[(Byte)InternalCmd.PublicCmdId.UploadProgCmdId])
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't upload.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion

        #region プログラム実行開始処理
        private void RecvProgExecCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            _RecvDataList[(Byte)InternalCmd.PublicCmdId.ProgExecCmdId] = inRecvDataList;
            _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.ProgExecCmdId] = true;
        }

        private void WaitProgExecCmdAck()
        {
            int timeoutCounter = 10;

            while (!_AckRecvFlag[(Byte)InternalCmd.PublicCmdId.ProgExecCmdId])
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't execute uploaded program.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion

        #region ユーザーデータ処理
        private void RecvUserData(IReadOnlyList<byte> inRecvDataList)
        {
            if (_UserDataList.Count < 1000)
            {
                List<Byte> recvDataList = new List<byte>(inRecvDataList);

                recvDataList.RemoveRange(0, 2);
                GCHandle gch = GCHandle.Alloc(recvDataList.ToArray(), GCHandleType.Pinned);
                UserDataFromEcu curUserData = (UserDataFromEcu)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(UserDataFromEcu));
                gch.Free();
                lock (_UserDataListLock)
                {
                    _UserDataList.Add(curUserData);
                }
            }
        }
        #endregion

        #region CAN 設定処理
        private void RecvSetCanCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            _RecvDataList[(Byte)InternalCmd.PublicCmdId.SetCanCmdId] = inRecvDataList;
            if (inRecvDataList.Count >= 2)
            {
                _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetCanCmdId] = true;
            }
        }

        private void WaitSetCanCmdAck()
        {
            int timeoutCounter = 10;

            while (!_AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetCanCmdId])
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't set CAN.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion

        #region SPI 設定処理
        private void RecvSetSpiCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            _RecvDataList[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId] = inRecvDataList;
            if (inRecvDataList.Count >= 2)
            {
                _AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId] = true;
            }
        }

        private void WaitSetSpiCmdAck()
        {
            int timeoutCounter = 10;

            while (!_AckRecvFlag[(Byte)InternalCmd.PublicCmdId.SetSpiCmdId])
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't set SPI.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UserDataFromEcu
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PublicConfig.UserByteNum)]
        public Byte[] UserData;
        public UInt32 MaxLoad5k;
        public UInt32 AveLoad5k;
        public UInt32 MaxLoad1k;
        public UInt32 CurLoad1k;
        public UInt32 MaxLoadMain;
        public UInt32 CurLoadMain;
        public UInt32 MaxLoadIntrptDis;

        public UserDataFromEcu()
        {
            UserData = new Byte[PublicConfig.UserByteNum];
        }

        public UserDataFromEcu Clone()
        {
            UserDataFromEcu tmp = new UserDataFromEcu();

            for (int idx = 0; idx < UserData.Length; idx++)
            {
                tmp.UserData[idx] = UserData[idx];
            }
            tmp.MaxLoad5k = MaxLoad5k;
            tmp.AveLoad5k = AveLoad5k;
            tmp.MaxLoad1k = MaxLoad1k;
            tmp.CurLoad1k = CurLoad1k;
            tmp.MaxLoadMain = MaxLoadMain;
            tmp.CurLoadMain = CurLoadMain;
            tmp.MaxLoadIntrptDis = MaxLoadIntrptDis;

            return tmp;
        }
    }
}
