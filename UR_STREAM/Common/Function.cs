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
}
