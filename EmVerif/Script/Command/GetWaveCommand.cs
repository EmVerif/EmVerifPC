using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Script.Command
{
    class GetWaveCommand : ICommand
    {
        private string _StopState;
        private List<UInt32> _InIdList;
        private List<UInt32> _MixOutIdList;
        private List<UInt32> _ThroughOutIdList;
        private string _FileName;
        private List<double> _InDataList;
        private List<double> _MixOutDataList;
        private List<double> _ThroughOutDataList;

        public GetWaveCommand(string inStop, string inFileName, string inInId, string inThroughOutId, string inMixOutId)
        {
            _StopState = inStop;
            _FileName = inFileName;
            _InIdList = new List<UInt32>();
            _MixOutIdList = new List<UInt32>();
            _ThroughOutIdList = new List<UInt32>();
            if (inInId != null)
            {
                _InIdList = inInId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            if (inMixOutId != null)
            {
                _MixOutIdList = inMixOutId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            if (inThroughOutId != null)
            {
                _ThroughOutIdList = inThroughOutId.Split(',').ToList().Select(id => Convert.ToUInt32(id)).ToList();
            }
            CheckParam(inInId, inMixOutId, inThroughOutId);
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
            // TODO: 波形データをセーブ
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
    }
}
