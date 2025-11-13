using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using S7.Net;
using System.Timers;

namespace PLCtoPLCviaVPN
{
    internal static class Program
    {
        static PLC plcMaster = new PLC("PLC Master", "");
        static PLC plcSlave = new PLC("PLC Slave", "");
        static System.Timers.Timer timer;

        // Các địa chỉ Offset
        const int DB_NUMBER = 1;
        const int MASTER_MOTOR_READ_OFFSET = 0;
        const int MASTER_CONTROL_WRITE_OFFSET = 12;
        const int SLAVE_CONTROL_READ_OFFSET = 0;
        const int SLAVE_MOTOR_WRITE_OFFSET = 12;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            timer = new System.Timers.Timer(500); // 500 ms
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            timer.Stop();
            plcMaster.Disconnected();
            plcSlave.Disconnected();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // 1. Master -> Slave
            MotorData motorData = plcMaster.ReadMotor(DB_NUMBER, MASTER_MOTOR_READ_OFFSET);
            if (motorData != null)
            {
                plcSlave.WriteMotor(motorData,DB_NUMBER,SLAVE_MOTOR_WRITE_OFFSET);
            }

            // 2. Slave -> Master
            ControlData controlData = plcSlave.ReadControl(DB_NUMBER, SLAVE_CONTROL_READ_OFFSET);
            if (controlData != null)
            {
                plcMaster.WriteControl(controlData, DB_NUMBER, MASTER_CONTROL_WRITE_OFFSET);
            }

        }

    }
}
