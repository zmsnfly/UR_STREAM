using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR_STREAM.Models
{
    public class KeyValueModel: ObservableObject
    {
        public KeyValueModel(DateTime _time, string _key, double _value)
        {
            Time = _time;
            Key = _key;
            Value = _value;
        }
        private DateTime time;
            public DateTime Time
            {
                get => time;
                set
                {
                    SetProperty(ref time, value);
                    TimeFormat = value.ToString("HH：mm：ss：ffff");
                }
            }

            private string timeFormat;
            public string TimeFormat { get => timeFormat; set => SetProperty(ref timeFormat, value); }

            private string key;
            public string Key { get => key; set => SetProperty(ref key, value); }

            private double value;
            public double Value { get => value; set => SetProperty(ref this.value, value); }
    }
}
