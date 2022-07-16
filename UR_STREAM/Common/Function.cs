using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR_STREAM.Common
{
    public class Function
    {
        public static double Radian2Degree(double radian)
        {
            double degree = radian * 180 / Math.PI;
            return degree;
        }
    }

    public class StreamHelper
    {
        public enum DataType
        {
            DEG,RAD,NUM
        };
        public StreamHelper(byte[] arrMsgRec)
        {
            ArrMsgRec = arrMsgRec;
            Time = DateTime.Now;
            Init(); 
        }

        private void Init()
        {
            KeyValuePairs = new Dictionary<string, KeyValuePair<int, DataType>>
            {
                {"J1", new KeyValuePair<int, DataType>(32, DataType.DEG)},
                {"J2", new KeyValuePair<int, DataType>(33, DataType.DEG)},
                {"J3", new KeyValuePair<int, DataType>(34, DataType.DEG)},
                {"J4", new KeyValuePair<int, DataType>(35, DataType.DEG)},
                {"J5", new KeyValuePair<int, DataType>(36, DataType.DEG)},
                {"J6", new KeyValuePair<int, DataType>(37, DataType.DEG)},
                {"X", new KeyValuePair<int, DataType>(56, DataType.NUM)},
                {"Y", new KeyValuePair<int, DataType>(57, DataType.NUM)},
                {"Z", new KeyValuePair<int, DataType>(58, DataType.NUM)},
                {"RX", new KeyValuePair<int, DataType>(59, DataType.RAD)},
                {"RY", new KeyValuePair<int, DataType>(60, DataType.RAD)},
                {"RZ", new KeyValuePair<int, DataType>(61, DataType.RAD)}
            };

        }

        public Dictionary<string, double> GenerateDic()
        {
            var dic = new Dictionary<string, double>();
            foreach (var keypair in KeyValuePairs)
            {
                dic.Add(keypair.Key, (double)GetData(keypair.Key, keypair.Value.Value));
            }
            return dic;
        }

        public double? GetData(string key, DataType type=0)
        {
            if(KeyValuePairs.Keys.Contains(key))
            {
                if(type == DataType.DEG)
                {
                    return GetDeg(KeyValuePairs[key].Key);
                }
                else if(type == DataType.RAD)
                {
                    return GetRad(KeyValuePairs[key].Key);
                }
                else
                {
                    return GetNum(KeyValuePairs[key].Key);
                }
            }
            else
            {
                return null;
            }
        }

        public DateTime GetTime()
        {
            return Time;
        }
        public DateTime Time;
        public double GetDeg(int index)
        {
            byte[] tem_arr = new byte[ArrMsgRec.Length];
            Array.Copy(ArrMsgRec, tem_arr, ArrMsgRec.Length);
            Array.Reverse(tem_arr);
            var ret = Function.Radian2Degree(BitConverter.ToDouble(tem_arr, tem_arr.Length - 4 - (index * 8)));
            return ret;
        }
        public double GetRad(int index)
        {
            byte[] tem_arr = new byte[ArrMsgRec.Length];
            Array.Copy(ArrMsgRec, tem_arr, ArrMsgRec.Length);
            Array.Reverse(tem_arr);
            var ret = BitConverter.ToDouble(tem_arr, tem_arr.Length - 4 - (index * 8));
            return ret;
        }
        public double GetNum(int index)
        {
            byte[] tem_arr = new byte[ArrMsgRec.Length];
            Array.Copy(ArrMsgRec, tem_arr, ArrMsgRec.Length);
            Array.Reverse(tem_arr);
            var ret = BitConverter.ToDouble(tem_arr, tem_arr.Length - 4 - (index * 8))*1000;
            return ret;
        }

        private Dictionary<string,KeyValuePair<int, DataType>> keyValuePairs;
        public Dictionary<string, KeyValuePair<int, DataType>> KeyValuePairs
        {
            get { return keyValuePairs; }
            set { keyValuePairs = value; }
        }


        public byte[] ArrMsgRec { get; set; }
    }
}
