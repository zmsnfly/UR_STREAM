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
        public StreamHelper(byte[] arrMsgRec)
        {
            ArrMsgRec = arrMsgRec;
            Time = DateTime.Now;
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

        public byte[] ArrMsgRec { get; set; }
    }
}
