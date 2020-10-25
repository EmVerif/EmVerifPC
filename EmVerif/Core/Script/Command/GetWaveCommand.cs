using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Script.Command
{
    class GetWaveCommand : IEmVerifCommand
    {
        private string _StopState;
        private List<UInt32> _AdIdList;
        private List<UInt32> _PwmIdList;
        private List<UInt32> _SpioutIdList;
        private string _FileName;
        private List<double> _AdDataList;
        private List<double> _PwmDataList;
        private List<double> _SpioutDataList;

        public GetWaveCommand(string inStop, string inFileName, string inAdId, string inSpioutId, string inPwmId)
        {
            _StopState = inStop;
            _AdIdList = new List<UInt32>();
            _PwmIdList = new List<UInt32>();
            _SpioutIdList = new List<UInt32>();
            if (inAdId != null)
            {
                _AdIdList = inAdId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            if (inPwmId != null)
            {
                _PwmIdList = inPwmId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            if (inSpioutId != null)
            {
                _SpioutIdList = inSpioutId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            if (
                (inAdId == null) &&
                (inPwmId == null) &&
                (inSpioutId == null)
            )
            {
                _AdIdList = Enumerable.Range(0, PublicConfig.AdChNum).Select(x => (UInt32)x).ToList();
                _PwmIdList = Enumerable.Range(0, PublicConfig.PwmChNum).Select(x => (UInt32)x).ToList();
                _SpioutIdList = Enumerable.Range(0, PublicConfig.SpioutChNum).Select(x => (UInt32)x).ToList();
            }
            CheckParam(inAdId, inPwmId, inSpioutId);
            _FileName = inFileName;
            _AdDataList = new List<double>();
            _PwmDataList = new List<double>();
            _SpioutDataList = new List<double>();
        }

        public void Boot(ControllerState inState)
        {
            _AdDataList = new List<double>();
            _PwmDataList = new List<double>();
            _SpioutDataList = new List<double>();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (ioState.CurrentState == _StopState)
            {
                outFinFlag = true;
            }
            else
            {
                _AdDataList.AddRange(ioState.CurrentAdDataList);
                _PwmDataList.AddRange(ioState.CurrentPwmDataList);
                _SpioutDataList.AddRange(ioState.CurrentSpioutDataList);
                outFinFlag = false;
            }

            return null;
        }

        public void Finally(ControllerState inState)
        {
            using (FileStream filStream = new FileStream(inState.WorkFolder + @"\" + _FileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter binWriter = new BinaryWriter(filStream))
            {
                binWriter.Write(CreateHeader());
                binWriter.Write(CreateWaveData());
            }
        }

        private void CheckParam(string inAdId, string inPwmId, string inSpioutId)
        {
            foreach (var id in _AdIdList)
            {
                if (PublicConfig.AdChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 AdId 設定エラー⇒" + inAdId + "\n" +
                        "AdId 設定値範囲は 0 以上 " + PublicConfig.AdChNum + " 未満。"
                    );
                }
            }
            foreach (var id in _PwmIdList)
            {
                if (PublicConfig.PwmChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 PwmId 設定エラー⇒" + inPwmId + "\n" +
                        "PwmId 設定値範囲は 0 以上 " + PublicConfig.PwmChNum + " 未満。"
                    );
                }
            }
            foreach (var id in _SpioutIdList)
            {
                if (PublicConfig.SpioutChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 SpioutId 設定エラー⇒" + inSpioutId + "\n" +
                        "SpioutId 設定値範囲は 0 以上 " + PublicConfig.SpioutChNum + " 未満。"
                    );
                }
            }
        }

        private byte[] CreateWaveData()
        {
            Int32 smpNum = _AdDataList.Count / PublicConfig.AdChNum;
            List<double> dataList = new List<double>();

            for (int smp = 0; smp < smpNum; smp++)
            {
                foreach (var id in _AdIdList)
                {
                    dataList.Add(_AdDataList[PublicConfig.AdChNum * smp + (int)id]);
                }
                foreach (var id in _SpioutIdList)
                {
                    dataList.Add(_SpioutDataList[PublicConfig.SpioutChNum * smp + (int)id]);
                }
                foreach (var id in _PwmIdList)
                {
                    dataList.Add(_PwmDataList[PublicConfig.PwmChNum * smp + (int)id]);
                }
            }

            byte[] dataArray = new byte[dataList.Count * 2];
            for (int smp = 0; smp < dataList.Count; smp++)
            {
                Int16 data = (Int16)Math.Max(Int16.MinValue, Math.Min(dataList[smp] * 32768, Int16.MaxValue));
                Array.Copy(BitConverter.GetBytes(data), 0, dataArray, smp * 2, 2);
            }

            return dataArray;
        }

        private byte[] CreateHeader()
        {
            byte[] Datas = new byte[44];
            Int32 chNum = _AdIdList.Count + _PwmIdList.Count + _SpioutIdList.Count;
            Int32 samplingHz = EmVerif.Core.Communication.PublicConfig.SamplingKhz * 1000;
            Int32 fileSize = 2 * (_AdDataList.Count / PublicConfig.AdChNum) * chNum + 44;

            Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, Datas, 0, 4);
            Array.Copy(BitConverter.GetBytes(fileSize - 8), 0, Datas, 4, 4);
            Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, Datas, 8, 4);
            Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, Datas, 12, 4);
            Array.Copy(BitConverter.GetBytes(16), 0, Datas, 16, 4);
            Array.Copy(BitConverter.GetBytes(1), 0, Datas, 20, 2);
            Array.Copy(BitConverter.GetBytes(chNum), 0, Datas, 22, 2);
            Array.Copy(BitConverter.GetBytes(samplingHz), 0, Datas, 24, 4);
            Array.Copy(BitConverter.GetBytes(2 * chNum * samplingHz), 0, Datas, 28, 4);
            Array.Copy(BitConverter.GetBytes(2 * chNum), 0, Datas, 32, 2);
            Array.Copy(BitConverter.GetBytes(16), 0, Datas, 34, 2);
            Array.Copy(Encoding.ASCII.GetBytes("data"), 0, Datas, 36, 4);
            Array.Copy(BitConverter.GetBytes(fileSize - 126), 0, Datas, 40, 4);

            return (Datas);
        }
    }
}
