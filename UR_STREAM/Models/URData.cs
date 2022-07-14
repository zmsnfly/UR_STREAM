using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR_STREAM.Models
{
    public class URData
    {
        public DateTime Time { get; set; }

        public Dictionary<string, double> KeyDic { get; set; }
    }
}
