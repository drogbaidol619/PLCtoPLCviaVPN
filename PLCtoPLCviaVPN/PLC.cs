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

        public PLC(string name, string ip)
        {
            Name = name;
            IP = ip;
            thePLC = new Plc(CpuType.S71200, IP, 0, 1);

            try
            {
                thePLC.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to PLC {Name} at {IP}: {ex.Message}");
            }
        }

        public void Disconnected()
        {
            if (thePLC.IsConnected)
            {
                thePLC.Close();
            }
        }

        public bool IsConnected()
        {
            return thePLC.IsConnected;
        }

        public MotorData ReadMotor(int db, int startOffset)
        {
            // lock để đảm bảo an toàn khi nhiều luồng cùng gọi
            lock (LockObject)
            {
                if (thePLC.IsConnected)
                {
                    // Đọc class MotorData tại DB, bắt đầu từ offset
                    return (MotorData)thePLC.ReadStruct(typeof(MotorData), db, startOffset);
                }
                return null; // Trả về null nếu không kết nối được
            }
        }

        public void WriteMotor(MotorData data, int db, int startOffset)
        {
            lock (LockObject)
            {
                if (thePLC.IsConnected && data != null)
                {
                    thePLC.WriteStruct(data, db, startOffset);
                }
            }
        }

    /// Đọc ControlData từ DB và Offset
        public ControlData ReadControl(int db, int startOffset)
        {
            lock (LockObject)
            {
                if (thePLC.IsConnected)
                {
                    return (ControlData)thePLC.ReadStruct(typeof(ControlData), db, startOffset);
                }
                return null;
            }
        }

    /// Ghi ControlData xuống DB và Offset
        public void WriteControl(ControlData data, int db, int startOffset)
        {
            lock (LockObject)
            {
                if (thePLC.IsConnected && data != null)
                {
                    thePLC.WriteStruct(data, db, startOffset);
                }
            }
        }
    }

    //////////Khai báo cấu trúc MotorData tương ứng với PLC

    public class MotorData
    {
        public bool Status { get; set; }
        public int Speed { get; set; }
        public float Voltage { get; set; }
        public float Temperature { get; set; }
    }
    /////////Khai báo cấu trúc ControlData tương ứng với PLC
    public class ControlData
    {
        public float Kp { get; set; }
        public float Ki { get; set; }
        public float Kd { get; set; }
    }

}
