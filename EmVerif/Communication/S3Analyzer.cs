using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace EmVerif.Communication
{
    class S3Analyzer
    {
        public static S3Analyzer Instance = new S3Analyzer();

        private string _S3Format = @"^S3(?<size>[0-9A-F]{2})(?<addr>[0-9A-F]{8})(?<data>[0-9A-F]*)(?<checksum>[0-9A-F]{2})$";
        private Dictionary<UInt32, Byte> _S3Data = new Dictionary<UInt32, Byte>();
        private UInt32 _MaxAddr = 0;
        private UInt32 _MinAddr = 0xFFFFFFFF;

        public void Clear()
        {
            _S3Data = new Dictionary<UInt32, Byte>();
            _MaxAddr = 0;
            _MinAddr = 0xFFFFFFFF;
        }

        public void Read(string inFileName)
        {
            StreamReader sr = new StreamReader(inFileName);

            Read(sr.ReadToEnd().Replace("\r", "").Split('\n').ToList());

            sr.Close();
        }

        public void Read(List<string> inContentList)
        {
            Clear();
            foreach (string oneLine in inContentList)
            {
                Match mc = Regex.Match(oneLine, _S3Format);

                if (mc.Success)
                {
                    int dataSize = Convert.ToInt32(mc.Groups["size"].Value, 16) - 5;
                    UInt32 addr = Convert.ToUInt32(mc.Groups["addr"].Value, 16);
                    string dataStr = mc.Groups["data"].Value;
                    int checksum = Convert.ToInt32(mc.Groups["checksum"].Value, 16);

                    for (int i = 0; i < dataSize; i++)
                    {
                        UInt32 curAddr = addr + (UInt32)i;
                        if (!_S3Data.ContainsKey(curAddr))
                        {
                            Byte data = Convert.ToByte(dataStr.Substring(i * 2, 2), 16);

                            _S3Data.Add(curAddr, data);
                        }
                    }
                    _MaxAddr = Math.Max(_MaxAddr, addr + (UInt32)dataSize - 1);
                    _MinAddr = Math.Min(_MinAddr, addr);
                }
            }
        }

        public UInt32 GetSize()
        {
            UInt32 ret;

            if (_MaxAddr >= _MinAddr)
            {
                ret = _MaxAddr - _MinAddr + 1;
            }
            else
            {
                ret = 0;
            }

            return ret;
        }

        public UInt32 GetTopAddr()
        {
            return _MinAddr;
        }

        public List<Byte> GetData()
        {
            return GetData(_MinAddr, GetSize());
        }

        public List<Byte> GetData(UInt32 inFromAddr, UInt32 inSize)
        {
            List<Byte> dataList = new List<byte>();
            UInt32 curAddr = inFromAddr;

            for (UInt32 i = 0; i < inSize; i++)
            {
                if (_S3Data.ContainsKey(curAddr))
                {
                    dataList.Add(_S3Data[curAddr]);
                }
                else
                {
                    dataList.Add(0x00);
                }
                curAddr++;
            }

            return dataList;
        }
    }
}
