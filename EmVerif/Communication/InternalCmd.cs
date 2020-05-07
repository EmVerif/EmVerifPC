﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace EmVerif.Communication
{
    delegate void Dg_Recv(IReadOnlyList<byte> inRecvDataList);

    class InternalCmd
    {
        public enum PublicCmdId
        {
            ClearEcuErrCmdId = 0x04,
            UploadProgCmdId = 0x05,
            ProgExecCmdId = 0x06,
            AuthCmdId = 0x08,
            SetCanCmdId = 0x09,
            SetSpiCmdId = 0x0A
        }

        public static InternalCmd Instance = new InternalCmd();
        public UInt32 PcRecvErrorCounter { get; private set; }
        public UInt32 EcuRecvErrorCounter { get; private set; }

        private const UInt32 _IfVersion = 0x00000000;

        private const Byte _StartCmdId = 0x01;
        private const Byte _EndCmdId = 0x02;
        private const Byte _PcAliveCmdId = 0x03;
        private const Byte _EcuAliveCmdId = 0x03;
        private const Byte _SendUserDataToEcuCmdId = 0x07;
        private const Byte _SendUserDataFromEcuCmdId = 0x07;

        private UInt32 _EmVerifVersion;

        private UInt32 _AlivePcTimeCounterMSec;
        private UInt32 _DeadEcuTimeCounterMSec;
        private Boolean _AliveEcuFlag;

        private Dg_Recv[] _RecvEvent = new Dg_Recv[Byte.MaxValue];

        private List<List<byte>> _SendCmdQueue;
        private object _SendCmdQueueLock = new object();

        private Boolean _StartCmdAckRecv;
        private Boolean _EndCmdAckRecv;

        private Byte _PcEcuCmdCounter;
        private Byte _EcuPcCmdCounter;

        public InternalCmd()
        {
            EtherCom.Instance.RecvEvent = AnalyzeRecvData;

            _AlivePcTimeCounterMSec = 0;
            _DeadEcuTimeCounterMSec = 0;
            _AliveEcuFlag = false;

            for (int idx = 0; idx < _RecvEvent.Length; idx++)
            {
                _RecvEvent[idx] = null;
            }

            _SendCmdQueue = new List<List<byte>>();

            _StartCmdAckRecv = false;
            _EndCmdAckRecv = false;

            _PcEcuCmdCounter = 0;
            _EcuPcCmdCounter = 0;

            PcRecvErrorCounter = 0;
            EcuRecvErrorCounter = 0;
        }

        public IEnumerable<IPAddress> GetIpV4List()
        {
            return EtherCom.Instance.GetIpV4List();
        }

        public void Start(IPAddress inIpAddr)
        {
            if (EtherCom.Instance.GetRecvTaskBusy())
            {
                throw new Exception("Already started.");
            }
            else
            {
                _SendCmdQueue = new List<List<byte>>();

                PcRecvErrorCounter = 0;
                EcuRecvErrorCounter = 0;
                _PcEcuCmdCounter = 0;
                _EcuPcCmdCounter = 0;
                _RecvEvent[_EcuAliveCmdId] = RecvEcuAliveCmd;
                EtherCom.Instance.Connect(inIpAddr);

                _AlivePcTimeCounterMSec = 0;
                _DeadEcuTimeCounterMSec = 0;
                MmTimer.Instance.OnTimer += SendCmdTask;
                MmTimer.Instance.OnTimer += AlivePc_Tick;
                MmTimer.Instance.OnTimer += DeadEcu_Tick;
                MmTimer.Instance.Start();

                SendStartCmd();
                WaitStartCmdAck();

                _AliveEcuFlag = false;
            }
        }

        public void SetRecvUserDataFromEcuEvent(Dg_Recv inRecvEvent)
        {
            _RecvEvent[_SendUserDataFromEcuCmdId] = inRecvEvent;
        }

        public void SendUserDataToEcu(IEnumerable<Byte> inData)
        {
            List<byte> cmd = new List<byte>();

            cmd.Add(_SendUserDataToEcuCmdId);
            cmd.AddRange(inData);
            lock (_SendCmdQueueLock)
            {
                _SendCmdQueue.Add(cmd);
            }
        }

        public void ExecCmd(PublicCmdId inCmdId, IEnumerable<Byte> inData, Dg_Recv inRecvEvent)
        {
            List<byte> cmd = new List<byte>();

            _RecvEvent[(int)inCmdId] = inRecvEvent;
            cmd.Add((Byte)inCmdId);
            cmd.AddRange(inData);
            lock (_SendCmdQueueLock)
            {
                _SendCmdQueue.Add(cmd);
            }
        }

        public void End()
        {
            if (EtherCom.Instance.GetRecvTaskBusy())
            {
                MmTimer.Instance.OnTimer -= AlivePc_Tick;
                MmTimer.Instance.OnTimer -= DeadEcu_Tick;

                SendEndCmd();
                WaitEndCmdAck();

                MmTimer.Instance.Stop();
                MmTimer.Instance.OnTimer -= SendCmdTask;
                EtherCom.Instance.FinishRequest();
                WaitUdpEnd();
            }
        }

        #region 受信処理
        private void AnalyzeRecvData(byte[] inRecvData)
        {
            if (inRecvData.Length >= 2)
            {
                byte cmd = inRecvData[1];

                _AliveEcuFlag = true;
                if (inRecvData[0] != _EcuPcCmdCounter)
                {
                    PcRecvErrorCounter++;
                    _EcuPcCmdCounter = inRecvData[0];
                }
                _RecvEvent[cmd]?.Invoke(inRecvData.Skip(2).ToList());
                _EcuPcCmdCounter++;
            }
        }
        #endregion

        #region 送信処理
        private void SendCmdTask(object sender, EventArgs e)
        {
            lock (_SendCmdQueueLock)
            {
                if (_SendCmdQueue.Count != 0)
                {
                    List<byte> cmdList = new List<byte>();

                    cmdList.Add(_PcEcuCmdCounter);
                    cmdList.AddRange(_SendCmdQueue[0]);
                    _SendCmdQueue.RemoveAt(0);
                    EtherCom.Instance.Send(cmdList.ToArray());
                    _PcEcuCmdCounter++;
                }
            }
        }
        #endregion

        #region タイマイベント
        private void AlivePc_Tick(object sender, EventArgs e)
        {
            _AlivePcTimeCounterMSec++;
            if (_AlivePcTimeCounterMSec > 500)
            {
                List<byte> cmd = new List<byte>();

                cmd.Add(_PcAliveCmdId);
                lock (_SendCmdQueueLock)
                {
                    _SendCmdQueue.Add(cmd);
                }
                _AlivePcTimeCounterMSec = 0;
            }
        }

        private void DeadEcu_Tick(object sender, EventArgs e)
        {
            if (_AliveEcuFlag)
            {
                _AliveEcuFlag = false;
                _DeadEcuTimeCounterMSec = 0;
            }
            else
            {
                _DeadEcuTimeCounterMSec++;
                if (_DeadEcuTimeCounterMSec > 2000)
                {
                    End();
                    // TODO:
                    //throw new Exception("ECU 停止");
                }
            }
        }
        #endregion

        #region スタートコマンド処理
        private void SendStartCmd()
        {
            List<byte> cmd = new List<byte>();

            _StartCmdAckRecv = false;
            _RecvEvent[_StartCmdId] = RecvStartCmdAck;
            cmd.Add(_StartCmdId);
            lock (_SendCmdQueueLock)
            {
                _SendCmdQueue.Add(cmd);
            }
        }

        private void WaitStartCmdAck()
        {
            int timeoutCounter = 10;

            while (!_StartCmdAckRecv)
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't start ECU.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }

        private void RecvStartCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            UInt32 ifVersion;

            if (inRecvDataList.Count >= 8)
            {
                ifVersion = (
                    ((UInt32)inRecvDataList[0] << 24) +
                    ((UInt32)inRecvDataList[1] << 16) +
                    ((UInt32)inRecvDataList[2] << 8) +
                    ((UInt32)inRecvDataList[3])
                );
                _EmVerifVersion = (
                    ((UInt32)inRecvDataList[4] << 24) +
                    ((UInt32)inRecvDataList[5] << 16) +
                    ((UInt32)inRecvDataList[6] << 8) +
                    ((UInt32)inRecvDataList[7])
                );
                if (ifVersion == _IfVersion)
                {
                    _StartCmdAckRecv = true;
                }
            }
        }
        #endregion

        #region エンドコマンド処理
        private void SendEndCmd()
        {
            List<byte> cmd = new List<byte>();

            _EndCmdAckRecv = false;
            _RecvEvent[_EndCmdId] = RecvEndCmdAck;
            cmd.Add(_EndCmdId);
            lock (_SendCmdQueueLock)
            {
                _SendCmdQueue.Add(cmd);
            }
        }

        private void WaitEndCmdAck()
        {
            int timeoutCounter = 10;

            while (!_EndCmdAckRecv)
            {
                if (timeoutCounter <= 0)
                {
                    break;
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }

        private void RecvEndCmdAck(IReadOnlyList<byte> inRecvDataList)
        {
            _EndCmdAckRecv = true;
        }

        private void WaitUdpEnd()
        {
            int timeoutCounter = 100;

            while (EtherCom.Instance.GetRecvTaskBusy())
            {
                if (timeoutCounter <= 0)
                {
                    throw new Exception("Can't end UDP receive task.");
                }
                timeoutCounter--;
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion

        private void RecvEcuAliveCmd(IReadOnlyList<byte> inRecvDataList)
        {
            if (inRecvDataList.Count >= 4)
            {
                EcuRecvErrorCounter = (
                    ((UInt32)inRecvDataList[0] << 24) +
                    ((UInt32)inRecvDataList[1] << 16) +
                    ((UInt32)inRecvDataList[2] <<  8) +
                    ((UInt32)inRecvDataList[3])
                );
            }
        }
    }
}