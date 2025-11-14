using System;
using System.Drawing;
using System.Windows.Forms;
using S7.Net;
using System.Timers; // Timer hệ thống

namespace PLCtoPLCviaVPN
{
    public partial class Form1 : Form
    {
        PLC plcMaster = new PLC("PLC Master", "192.168.0.1"); 
        PLC plcSlave = new PLC("PLC Slave", "192.168.0.2");     
        System.Timers.Timer timer;

        bool _isBusy = false;

        // Các địa chỉ Offset
        const int MASTER_MOTOR_READ_OFFSET = 0;
        const int MASTER_CONTROL_WRITE_OFFSET = 0;
        const int SLAVE_CONTROL_READ_OFFSET = 0;
        const int SLAVE_MOTOR_WRITE_OFFSET = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cấu hình Timer
            timer = new System.Timers.Timer(500); 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Start(); 
        }

        // Sự kiện khi tắt Form
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
            plcMaster.Disconnected();
            plcSlave.Disconnected();
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;

            try
            {
                // === 1. ĐỌC PHẢN HỒI: TỪ SLAVE (DB1) -> VỀ MASTER (DB3) ===

                MotorData motorData = plcSlave.ReadMotor(1, SLAVE_MOTOR_WRITE_OFFSET);

                Console.WriteLine($"[SLAVE (DB1) -> MASTER (DB3)] Speed: {motorData.Speed}, Temp: {motorData.Temperature}");
                if (motorData != null)
                {
                    plcMaster.WriteMotor(motorData, 3, MASTER_MOTOR_READ_OFFSET);

                    // Cập nhật giao diện 
                    this.Invoke(new Action(() =>
                    {
                        lbSpeed.Text = motorData.Speed.ToString();
                        lbTemperature.Text = motorData.Temperature.ToString("0.0") + " °C";
                        if (motorData.Status) 
                        {
                            btStart.BackColor = Color.Green;
                            btStop.BackColor = Color.LightGray;
                        }
                        else 
                        {
                            btStart.BackColor = Color.LightGray;
                            btStop.BackColor = Color.Red;
                        }
                    }));
                }
                else
                {
                    Console.WriteLine("[LỖI] Khong doc duoc tu Slave (DB1).");
                }

                    // === 2. GỬI LỆNH: TỪ MASTER (DB4) -> XUỐNG SLAVE (DB2) ===
                    ControlData controlData = plcMaster.ReadControl(4, MASTER_CONTROL_WRITE_OFFSET);

                if (controlData != null)
                {
                    plcSlave.WriteControl(controlData, 2, SLAVE_CONTROL_READ_OFFSET);
                    Console.WriteLine($"[MASTER (DB4) -> SLAVE (DB2)] Kp: {controlData.Kp}, Ki: {controlData.Ki}");
                    // Cập nhật giao diện 
                    this.Invoke(new Action(() =>
                    {
                        lbKp.Text = controlData.Kp.ToString("0.0");
                        lbKi.Text = controlData.Ki.ToString("0.0");
                        lbKd.Text = controlData.Kd.ToString("0.0");
                    }));
                }
                else
                {
                    Console.WriteLine("[LỖI] Khong doc duoc tu Master (DB4).");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi: " + ex.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            SendCommand(true);
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            SendCommand(false);
        }

        private void SendCommand(bool status)
        {
            try
            {
                MotorData cmd = new MotorData();
                cmd.Status = status;

                if (int.TryParse(tbSpeed.Text, out int setSpeed))
                {
                    cmd.Speed = setSpeed;
                }
                else
                {
                    cmd.Speed = 0;
                }

                // === SỬA LỖI LOGIC ===
                plcSlave.WriteMotor(cmd, 2, SLAVE_MOTOR_WRITE_OFFSET);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi lệnh: " + ex.Message);
            }
        }
    }
}