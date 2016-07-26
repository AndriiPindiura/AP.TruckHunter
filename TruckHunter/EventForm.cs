using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace TruckHunter
{
    public partial class EventForm : Form
    {
        public EventForm()
        {
            CamMonitor.CamMonitorEventHandler = new CamMonitor.CamMonitorEvent(this.GetParams);
            InitializeComponent();
        }

        private int entercam, exitcam, huntcam, _compress;

        private string serverip, id1c, truckid, culture, arm, entry, interval;

        private DateTime entertime, exittime, hunttime;

        private DateTime currentUpTime, currentEnterTime, currentExitTime;

        private int currentUpCam, currentEnterCam, currentExitCam;

        private bool swap = false;

        void GetParams(string ip, int incam, int outcam, int upcam, DateTime starttime, DateTime stoptime, DateTime uptime, 
            string ttn, string nomer, string kultura, string mesto, string projezd, string dt, bool flag, Color brush, int _quality)
        {
            _compress = _quality;
            swap = false;
            panel1.BackColor = brush;
            serverip = ip;
            entercam = incam;
            exitcam = outcam;
            huntcam = upcam;
            entertime = starttime;
            exittime = stoptime;
            hunttime = uptime;
            id1c = ttn;
            truckid = nomer;
            culture = kultura;
            arm = mesto;
            entry = projezd;
            interval = dt;
            textBox1.Text = entry;
            textBox2.Text = id1c;
            textBox3.Text = truckid;
            textBox4.Text = culture;
            textBox5.Text = arm;
            textBox6.Text = interval;
            /*upOCX.Width = (this.Width / 2);
            upOCX.Height = (this.Height / 2);
            upOCX.Top = 0;
            upOCX.Left = upOCX.Width;
            enterOCX.Width = (this.Width / 2);
            enterOCX.Height = (this.Height / 2);
            enterOCX.Top = upOCX.Top + upOCX.Height;
            enterOCX.Left = 0;
            exitOCX.Width = (this.Width / 2);
            exitOCX.Height = (this.Height / 2);
            exitOCX.Top = enterOCX.Top;
            exitOCX.Left = upOCX.Left;*/
            if (flag)
            {
                button1.Enabled = false;
                upOCX.Disconnect();
                enterOCX.Disconnect();
                exitOCX.Disconnect();
            }

            if (upOCX.IsConnected() == 1)
            {
                upOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + huntcam.ToString() + ">");
                upOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + huntcam.ToString() + ">,date<" + hunttime.ToString("dd-MM-yy") + ">,time<" + hunttime.ToString("HH:mm:ss") + ">");
                if (Properties.Settings.Default.play)
                {
                    upOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
                }
            }
            else
            {
                upOCX.Connect(serverip, "", "", "", 0);
            }
            if (entercam > 0)
                if (enterOCX.IsConnected() == 1)
                {
                    enterOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + entercam.ToString() + ">");
                    enterOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + entercam.ToString() + ">,date<" + entertime.ToString("dd-MM-yy") + ">,time<" + entertime.ToString("HH:mm:ss") + ">");
                    if (Properties.Settings.Default.play)
                    {
                        enterOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
                    }
                }
                else
                {
                    enterOCX.Connect(serverip, "", "", "", 0);
                }
            if (exitcam > 0)
                if (exitOCX.IsConnected() == 1)
                {
                    exitOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + exitcam.ToString() + ">");
                    exitOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitcam.ToString() + ">,date<" + exittime.ToString("dd-MM-yy") + ">,time<" + exittime.ToString("HH:mm:ss") + ">");
                    if (Properties.Settings.Default.play)
                    {
                        exitOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
                    }            
                }
                else
                {
                    exitOCX.Connect(serverip, "", "", "", 0);
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Bounds = Screen.PrimaryScreen.WorkingArea;
            this.Icon = this.Owner.Icon;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.MaximizeBox = false;
            //MessageBox.Show(panel2.Width.ToString() + "x" + panel2.Height.ToString());
            this.Text = "HuntMonitor";
            this.Owner.Hide();
            upOCX.Width = (this.Width / 2);
            upOCX.Height = (this.Height / 2);
            upOCX.Top = 0;
            upOCX.Left = upOCX.Width;
            enterOCX.Width = (this.Width / 2);
            enterOCX.Height = (this.Height / 2);
            enterOCX.Top = upOCX.Top + upOCX.Height;
            enterOCX.Left = 0;
            exitOCX.Width = (this.Width / 2);
            exitOCX.Height = (this.Height / 2);
            exitOCX.Top = enterOCX.Top;
            exitOCX.Left = upOCX.Left;
            CamConnect(serverip, entercam, exitcam, huntcam);
            panel1.Width = upOCX.Width;
            panel1.Height = upOCX.Height;
            panel1.Top = 0;
            panel1.Left = 0;
            textBox1.Text = entry;
            textBox2.Text = id1c;
            textBox3.Text = truckid;
            textBox4.Text = culture;
            textBox5.Text = arm;
            textBox6.Text = interval;
            button2.Focus();
        }

        private void CamShow(int cam1, int cam2, int cam3)
        {
            if (cam1 > 0)
                upOCX.ShowCam(cam1, _compress, 1);
            if (cam2 > 0)
                enterOCX.ShowCam(cam2, _compress, 1);
            if (cam3 > 0)
                exitOCX.ShowCam(cam3, _compress, 1);
            timer2.Enabled = true;
        }

        private void upOCX_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            if(e.state == 1)
            {
                button1.Enabled = true;
                button6.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                timer1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                button6.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            
        }

        private void CamConnect(string server, int cam1, int cam2, int cam3)
        {
           upOCX.Connect(server, "", "", "", 0);
           enterOCX.Connect(server, "", "", "", 0);
           exitOCX.Connect(server, "", "", "", 0);
     
        }

        private void ArchGo(int cam1, int cam2, int cam3, DateTime dat1, DateTime dat2, DateTime dat3)
        {
            string DoReact = "";
            DoReact = "MONITOR||ARCH_FRAME_TIME|cam<" + cam1.ToString() + ">,date<" + dat1.ToString("dd-MM-yy") + ">,time<" + dat1.ToString("HH:mm:ss") + ">";
            upOCX.DoReactMonitor(DoReact);
            DoReact = "MONITOR||ARCH_FRAME_TIME|cam<" + cam2.ToString() + ">,date<" + dat2.ToString("dd-MM-yy") + ">,time<" + dat2.ToString("HH:mm:ss") + ">";
            if (cam2 > 0)
                enterOCX.DoReactMonitor(DoReact);
            DoReact = "MONITOR||ARCH_FRAME_TIME|cam<" + cam3.ToString() + ">,date<" + dat3.ToString("dd-MM-yy") + ">,time<" + dat3.ToString("HH:mm:ss") + ">";
            if (cam3 > 0)
                exitOCX.DoReactMonitor(DoReact);
            if (Properties.Settings.Default.play)
            {
                upOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
                enterOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
                exitOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            CamShow(huntcam,entercam,exitcam);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { this.Hide(); }
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            upOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<STOP>");
            enterOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<STOP>");
            exitOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<STOP>");
            this.Hide();
            this.Owner.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CamShow(huntcam, entercam, exitcam);
            ArchGo(huntcam, entercam, exitcam, hunttime, entertime, exittime);
            textBox1.Text = entry;
            textBox2.Text = id1c;
            textBox3.Text = truckid;
            textBox4.Text = culture;
            textBox5.Text = arm;
            textBox6.Text = interval;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            string tpl = "";
            tpl = Environment.CurrentDirectory.ToString() + @"\export";
            upOCX.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + huntcam.ToString() + ">,file<" + tpl + @"\" + huntcam.ToString() + "_" + hunttime.ToString("yyyyMMddHHmmss") + ".jpg>");
            if (entercam > 0)
                enterOCX.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + entercam.ToString() + ">,file<" + tpl + @"\" + entercam.ToString() + "_" + entertime.ToString("yyyyMMddHHmmss") + ".jpg>");
            if (exitcam > 0)
                exitOCX.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + exitcam.ToString() + ">,file<" + tpl + @"\" + exitcam.ToString() + "_" + exittime.ToString("yyyyMMddHHmmss") + ".jpg>");

        }

        private void timer2_Tick_1(object sender, EventArgs e)
        {
            ArchGo(huntcam, entercam, exitcam, hunttime, entertime, exittime);
            timer2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            upOCX.DoReactMonitor(textBox5.Text);
        }

        private void panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { this.Hide(); }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            upOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
            enterOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
            exitOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PLAY_NONSTOP>");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            upOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PAUSE>");
            enterOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PAUSE>");
            exitOCX.DoReactMonitor("MONITOR||KEY_PRESSED|key<PAUSE>");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "Нарушение логики маятника")
            {
                Navigator.NavigatorEventHandler(true, 2);
            }
            else
            {
                Navigator.NavigatorEventHandler(true, 1);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "Нарушение логики маятника")
            {
                Navigator.NavigatorEventHandler(false, 2);
            }
            else
            {
                Navigator.NavigatorEventHandler(false, 1);
            }
        }

        private void panel1_BackColorChanged(object sender, EventArgs e)
        {
            this.textBox1.BackColor = this.panel1.BackColor;
            this.textBox2.BackColor = this.panel1.BackColor;
            this.textBox3.BackColor = this.panel1.BackColor;
            this.textBox4.BackColor = this.panel1.BackColor;
            this.textBox5.BackColor = this.panel1.BackColor;
            this.textBox6.BackColor = this.panel1.BackColor;

        }


        private void upOCX_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            currentUpTime = e.date;
            currentUpCam = e.cam_id;
        }

        private void exitOCX_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            currentExitTime = e.date;
            currentExitCam = e.cam_id;
        }

        private void enterOCX_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            currentEnterTime = e.date;
            currentEnterCam = e.cam_id;
        }

        private void upOCX_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.ControlKey && currentUpCam > 0)
            {
                if (e.button == 1)
                {
                    upOCX.Width = this.Width;
                    upOCX.Height = this.Height;
                    upOCX.BringToFront();
                    upOCX.Top = 0;
                    upOCX.Left = 0;
                    upOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentUpCam + ">,date<" + currentUpTime.ToString("dd-MM-yy") + ">,time<" + currentUpTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    upOCX.Width = (this.Width / 2);
                    upOCX.Height = (this.Height / 2);
                    upOCX.Top = 0;
                    upOCX.Left = upOCX.Width;
                    upOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentUpCam + ">,date<" + currentUpTime.ToString("dd-MM-yy") + ">,time<" + currentUpTime.ToString("HH:mm:ss") + ">");
                    upOCX.SendToBack();
                }
            }
        }

        private void exitOCX_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.ControlKey && currentExitCam > 0)
            {
                if (e.button == 1)
                {
                    exitOCX.Width = this.Width;
                    exitOCX.Height = this.Height;
                    exitOCX.BringToFront();
                    exitOCX.Top = 0;
                    exitOCX.Left = 0;
                    exitOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentExitCam + ">,date<" + currentExitTime.ToString("dd-MM-yy") + ">,time<" + currentExitTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    exitOCX.Width = (this.Width / 2);
                    exitOCX.Height = (this.Height / 2);
                    exitOCX.Top = enterOCX.Top;
                    exitOCX.Left = upOCX.Left;
                    exitOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentExitCam + ">,date<" + currentExitTime.ToString("dd-MM-yy") + ">,time<" + currentExitTime.ToString("HH:mm:ss") + ">");
                    exitOCX.SendToBack();
                }
            }
        }

        private void enterOCX_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.ControlKey && currentEnterCam > 0)
            {
                if (e.button == 1)
                {
                    enterOCX.Width = this.Width;
                    enterOCX.Height = this.Height;
                    enterOCX.BringToFront();
                    enterOCX.Top = 0;
                    enterOCX.Left = 0;
                    enterOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentEnterCam + ">,date<" + currentEnterTime.ToString("dd-MM-yy") + ">,time<" + currentEnterTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    enterOCX.Width = (this.Width / 2);
                    enterOCX.Height = (this.Height / 2);
                    enterOCX.Top = upOCX.Top + upOCX.Height;
                    enterOCX.Left = 0;
                    enterOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + currentEnterCam + ">,date<" + currentEnterTime.ToString("dd-MM-yy") + ">,time<" + currentEnterTime.ToString("HH:mm:ss") + ">");
                    enterOCX.SendToBack();
                }
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            if (swap)
            {
                swap = false;
                upOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + huntcam.ToString() + ">");
                upOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + huntcam.ToString() + ">,date<" + hunttime.ToString("dd-MM-yy") + ">,time<" + hunttime.ToString("HH:mm:ss") + ">");
                enterOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + entercam.ToString() + ">");
                enterOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + entercam.ToString() + ">,date<" + entertime.ToString("dd-MM-yy") + ">,time<" + entertime.ToString("HH:mm:ss") + ">");
                exitOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + exitcam.ToString() + ">");
                exitOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitcam.ToString() + ">,date<" + exittime.ToString("dd-MM-yy") + ">,time<" + exittime.ToString("HH:mm:ss") + ">");
            }
            else
            {
                swap = true;
                upOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + huntcam.ToString() + ">");
                upOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + huntcam.ToString() + ">,date<" + hunttime.ToString("dd-MM-yy") + ">,time<" + hunttime.ToString("HH:mm:ss") + ">");
                enterOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + exitcam.ToString() + ">");
                enterOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitcam.ToString() + ">,date<" + entertime.ToString("dd-MM-yy") + ">,time<" + entertime.ToString("HH:mm:ss") + ">");
                exitOCX.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + entercam.ToString() + ">");
                exitOCX.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + entercam.ToString() + ">,date<" + exittime.ToString("dd-MM-yy") + ">,time<" + exittime.ToString("HH:mm:ss") + ">");
            }
        }

    }
}