using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;   
using System.Timers;

namespace PLCtoPLCviaVPN
{
    public class PLC
    {
        public string Name;
        public string IP;
        public Plc thePLC;
        public object LockObject = new object();


    }

    public class MotorData
    {
        public bool Status { get; set; }
        public int Speed { get; set; }
        public float Voltage { get; set; }
        public float Temperature { get; set; }
    }

    public class ControlData
    {
        public float Kp { get; set; }
        public float Ki { get; set; }
        public float Kd { get; set; }
    }


}
