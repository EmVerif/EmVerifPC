using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace EmVerif.Communication
{
    delegate void Dg_RecvEvent(byte[] inRecvData);

    class EtherCom
    {
        public Dg_RecvEvent RecvEvent;
        public static EtherCom Instance = new EtherCom();

        private UdpClient _UdpClient;
        private string _ServerIpAddr = "192.168.0.100";
        private string _UpperIpAddr = "192.168.0.";
        private int _ServerPort = 2000;
        private int _MyPort = 2001;
        private Task _RecvTask;

        private Boolean _FinishRequest;

        public IEnumerable<IPAddress> GetIpV4List()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            List<IPAddress> ipAddrList = new List<IPAddress>();

            foreach (NetworkInterface ni in nis)
            {
                foreach (UnicastIPAddressInformation a in ni.GetIPProperties().UnicastAddresses)
                {
                    if (a.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ipStr = a.Address.ToString();
                        if (!ipStr.Contains(_ServerIpAddr) && ipStr.Contains(_UpperIpAddr))
                        {
                            ipAddrList.Add(a.Address);
                        }
                    }
                }
            }

            return ipAddrList;
        }

        public void Connect(IPAddress inIpAddr)
        {
            IPEndPoint localEP = new IPEndPoint(inIpAddr, _MyPort);

            _UdpClient = new UdpClient(localEP);
            _UdpClient.Client.ReceiveBufferSize = 1048576;
            _FinishRequest = false;
            _RecvTask = new Task(() => RecvTask());
            _RecvTask.Start();
        }

        public void FinishRequest()
        {
            _FinishRequest = true;
            _UdpClient.Close();
            _UdpClient.Dispose();
        }

        public bool GetRecvTaskBusy()
        {
            bool ret;

            if (_RecvTask == null)
            {
                ret = false;
            }
            else
            {
                ret = (_RecvTask.Status == TaskStatus.Running);
            }
            return ret;
        }

        public void Send(Byte[] inSendData)
        {
            //IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(_ServerIpAddr), _ServerPort);

            //_UdpClient.BeginSend(inSendData, inSendData.Length, serverEP, null, null);
            _UdpClient.Send(inSendData, inSendData.Length, _ServerIpAddr, _ServerPort);
        }

        #region バックグラウンド処理
        private void RecvTask()
        {
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(_ServerIpAddr), _ServerPort);

            while (_FinishRequest == false)
            {
                try
                {
                    byte[] recvBytes = _UdpClient.Receive(ref serverEP);
                    RecvEvent(recvBytes);
                }
                catch
                {
                }
            }
        }
        #endregion
    }
}
