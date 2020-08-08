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
        private List<Int32> _InIdList;
        private List<Int32> _MixOutIdList;
        private List<Int32> _ThroughOutIdList;
        private string _FileName;
        private List<double> _InDataList;
        private List<double> _MixOutDataList;
        private List<double> _ThroughOutDataList;

        public GetWaveCommand(string inStop, string inFileName, string inInId, string inThroughOutId, string inMixOutId)
        {
            _StopState = inStop;
            _InIdList = new List<Int32>();
            _MixOutIdList = new List<Int32>();
            _ThroughOutIdList = new List<Int32>();
            if (inInId != null)
            {
                _InIdList = inInId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            }
            if (inMixOutId != null)
            {
                _MixOutIdList = inMixOutId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            }
            if (inThroughOutId != null)
            {
                _ThroughOutIdList = inThroughOutId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            }
            CheckParam(inInId, inMixOutId, inThroughOutId);
            _FileName = inFileName;
            _InDataList = new List<double>();
            _MixOutDataList = new List<double>();
            _ThroughOutDataList = new List<double>();
        }

        public void Boot(ControllerState inState)
        {
            _InDataList = new List<double>();
            _MixOutDataList = new List<double>();
            _ThroughOutDataList = new List<double>();
        }

        public string ExecPer10Ms(ControllerState ioState, out bool outFinFlag)
        {
            if (ioState.CurrentState == _StopState)
            {
                outFinFlag = true;
            }
            else
            {
                _InDataList.AddRange(ioState.CurrentInDataList);
                _MixOutDataList.AddRange(ioState.CurrentMixOutDataList);
                _ThroughOutDataList.AddRange(ioState.CurrentThroughOutDataList);
                outFinFlag = false;
            }

            return null;
        }

        public void Finally()
        {
            using (FileStream filStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter binWriter = new BinaryWriter(filStream))
            {
                binWriter.Write(CreateHeader());
                binWriter.Write(CreateWaveData());
            }
        }

        private void CheckParam(string inInId, string inMixOutId, string inThroughOutId)
        {
            foreach (var id in _InIdList)
            {
                if (PublicConfig.InChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 InId 設定エラー⇒" + inInId + "\n" +
                        "InId 設定値範囲は 0 以上 " + PublicConfig.InChNum + " 未満。"
                    );
                }
            }
            foreach (var id in _MixOutIdList)
            {
                if (PublicConfig.MixOutChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 MixOutId 設定エラー⇒" + inMixOutId + "\n" +
                        "MixOutId 設定値範囲は 0 以上 " + PublicConfig.MixOutChNum + " 未満。"
                    );
                }
            }
            foreach (var id in _ThroughOutIdList)
            {
                if (PublicConfig.ThroughOutChNum <= id)
                {
                    throw new Exception(
                        "GetWave コマンド内 ThroughOutId 設定エラー⇒" + inThroughOutId + "\n" +
                        "ThroughOutId 設定値範囲は 0 以上 " + PublicConfig.ThroughOutChNum + " 未満。"
                    );
                }
            }
        }

        private byte[] CreateWaveData()
        {
            Int32 smpNum = _InDataList.Count / PublicConfig.InChNum;
            List<double> dataList = new List<double>();

            for (int smp = 0; smp < smpNum; smp++)
            {
                foreach (var id in _InIdList)
                {
                    dataList.Add(_InDataList[PublicConfig.InChNum * smp + (int)id]);
                }
                foreach (var id in _ThroughOutIdList)
                {
                    dataList.Add(_ThroughOutDataList[PublicConfig.ThroughOutChNum * smp + (int)id]);
                }
                foreach (var id in _MixOutIdList)
                {
                    dataList.Add(_MixOutDataList[PublicConfig.MixOutChNum * smp + (int)id]);
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
            Int32 chNum = _InIdList.Count + _MixOutIdList.Count + _ThroughOutIdList.Count;
            Int32 samplingHz = EmVerif.Core.Communication.PublicConfig.SamplingKhz * 1000;
            Int32 fileSize = 2 * (_InDataList.Count / PublicConfig.InChNum) * chNum + 44;

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
