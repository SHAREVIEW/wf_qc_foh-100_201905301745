using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace wf_qc_foh_100
{
    public partial class Form1 : Form
    {
        List<string> expressions = new List<string>();  //save config

        //telnet part copy
        public static NetworkStream stream;
        public static TcpClient tcpclient;
        //private readonly int BuffSize = 1024 * 4;

        private bool isDeviceConnected = false;

        //Device Connect state 
        public bool IsDeviceConnected
        {
            get { return isDeviceConnected; }
            set
            {
                isDeviceConnected = value;
                if (isDeviceConnected)
                {
                    ShowStatusBar("The device is connected !!", true);
                    btnConnect.Text = "Disconnect";
                    ToggleControls(true);
                }
                else
                {
                    tcpclient.Close();
                    ShowStatusBar("The device is diconnected !!", true);
                    btnConnect.Text = "Connect";
                    ToggleControls(false);
                }
            }
        }

        //ToggleControls
        private void ToggleControls(bool value)
        {
            //btnDeviceInfo.Enabled = value;
            //  btnGetONU.Enabled = value;
            //  btnGetLoid.Enabled = value;
            //    btnGetFPGAVersion.Enabled = value;
          //  btnUpgradeGPON.Enabled = value;
        //    btnUpgradeEPON.Enabled = value;
            //  btnLogData.Enabled = value;
            //  btnRestartDevice.Enabled = value;
            //  btnLogin.Enabled = value;
            //btnAbout.Enabled = value;
            //btnSendCommand.Enabled = value;
            //  tbxSendCommand.Enabled = value;
            tbxPort.Enabled = !value;
            tbxDeviceIP.Enabled = !value;
            tbxLogin.Enabled = !value;
            tbxPassword.Enabled = !value;
            // Color.FromArgb(204, 204, 204)
            //rtbShowInfo.BackColor = Color.FromArgb(204, 204, 204);
            //tbxSendCommand.BackColor = Color.FromArgb(245, 245, 243);
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        { UniversalStatic.DrawLineInFooter(pnlHeader, Color.FromArgb(204, 204, 204), 2); }
        //telnet part copy

        public Form1()
        {
            InitializeComponent();

            //telnet part copy
            ToggleControls(false);
            ShowStatusBar(string.Empty, true);
            //telnet part copy
            ReadString();
        }

        //telnet part copy
        public void ShowStatusBar(string message, bool type)
        {
            if (message.Trim() == string.Empty)
            {
                lblStatus.Visible = false;
                return;
            }

            lblStatus.Visible = true;
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.White;

            if (type)
            {
                lblStatus.BackColor = Color.FromArgb(79, 208, 154);
                lblHeader.ForeColor = Color.FromArgb(79, 208, 154);
            }
            else
            {
                lblStatus.BackColor = Color.FromArgb(230, 112, 134);
                lblHeader.ForeColor = Color.FromArgb(230, 112, 134);
            }

        }

        //btnConnect_Click

        private void btnConnect_Click(object sender, EventArgs e)
        {

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ShowStatusBar(string.Empty, true);

                if (IsDeviceConnected)
                {
                    IsDeviceConnected = false;
                    this.Cursor = Cursors.Default;

                    return;
                }

                string ipAddress = tbxDeviceIP.Text.Trim();
                string port = tbxPort.Text.Trim();
                string login = tbxLogin.Text.Trim();
                string password = tbxPassword.Text.Trim();
                if (ipAddress == string.Empty || port == string.Empty || login == string.Empty || password == string.Empty)
                    throw new Exception("The Device IP Port Login and Password is mandotory !!");

                //int portNumber = 23;
                //if (!int.TryParse(port, out portNumber))
                //    throw new Exception("Not a valid port number");

                bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                if (!isValidIpA)
                    throw new Exception("The Device IP is invalid !!");

                isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                if (!isValidIpA)
                    throw new Exception("The device at " + ipAddress + ":" + port + " did not respond!!");

                tcpclient = new TcpClient(ipAddress, int.Parse(port));  // connect server
                stream = tcpclient.GetStream();   // get net stream
                IsDeviceConnected = true;

                //objZkeeper = new ZkemClient(RaiseDeviceEvent);
                //IsDeviceConnected = objZkeeper.Connect_Net(ipAddress, portNumber);
                //string result = string.Empty;
                if (IsDeviceConnected && stream.DataAvailable)     //connected and receive data
                {
                    Byte[] output = new Byte[1024];
                    String responseoutput = String.Empty;
                    Byte[] cmd = System.Text.Encoding.ASCII.GetBytes(""); //"\n"
                    stream.Write(cmd, 0, cmd.Length);

                    cmd = System.Text.Encoding.ASCII.GetBytes("get device info" + "\r\n");       //first try to get info 
                    stream.Write(cmd, 0, cmd.Length);

                    Thread.Sleep(100);
                    Int32 bytes = stream.Read(output, 0, output.Length);
                    responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
                    // rtbShowInfo.Text += responseoutput;

                    Regex objToMatch = new Regex("Login:");
                    if (objToMatch.IsMatch(responseoutput))
                    {
                        cmd = System.Text.Encoding.ASCII.GetBytes(login + "\r\n");
                        stream.Write(cmd, 0, cmd.Length);
                    }
                    Thread.Sleep(100);
                    bytes = stream.Read(output, 0, output.Length);
                    responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
                    //  rtbShowInfo.Text += responseoutput;

                    objToMatch = new Regex("Passwd:");
                    if (objToMatch.IsMatch(responseoutput))
                    {
                        cmd = System.Text.Encoding.ASCII.GetBytes(password + "\r\n");
                        stream.Write(cmd, 0, cmd.Length);
                    }
                    Thread.Sleep(100);
                    bytes = stream.Read(output, 0, output.Length);
                    responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
                    // rtbShowInfo.Text += responseoutput;

                    objToMatch = new Regex("-->");
                    if (objToMatch.IsMatch(responseoutput))
                    {
                        cmd = System.Text.Encoding.ASCII.GetBytes("get device info" + "\r\n");
                        stream.Write(cmd, 0, cmd.Length);
                    }
                    Thread.Sleep(100);
                    bytes = stream.Read(output, 0, output.Length);
                    responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
                    //rtbShowInfo.Text += responseoutput;





                    //save info log
                    StreamWriter file = new StreamWriter("log.txt", true);
                    file.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "        " + responseoutput);
                    file.Close();                                                    
                }
            }
            catch (Exception ex)
            {
                ShowStatusBar(ex.Message, false);
                //rtbShowInfo.Text += "Device has been connected, and connot be connected twice at one time!!";
            }
            this.Cursor = Cursors.Default;
            Thread.Sleep(200);
        }

        #region   read & write config info
        private void writeInfo() {
            //save config log
            try
            {
                FileStream SaveFile = new FileStream(System.Environment.CurrentDirectory + "\\info.txt", FileMode.Append);   
                StreamWriter streamWriter = new StreamWriter(SaveFile);                // StreamWriter streamWriter = new StreamWriter(history.txt,true);
                foreach (string a in expressions)                     
                {
                    streamWriter.WriteLine(a);
                }
                streamWriter.Close();                  
            }
            catch (IOException ex)
            {
                MessageBox.Show("An IO exception has been thrown!");
                MessageBox.Show(ex.ToString());
            }
        }
        //  tBx_EponSoftware    tBx_EponFPGA    tBx_GponSoftware    tBx_GponFPGA
        private void ReadString()                     
        {
            try                                                                                                     
            {
                FileStream afile = new FileStream(System.Environment.CurrentDirectory + "\\info.txt", FileMode.OpenOrCreate);   
                StreamReader sr = new StreamReader(afile);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("EPON"))   //EPON
                    {
                        string[] sline = line.Trim().Split(' ');    //
                        tBx_EponSoftware.Text = sline[2];
                        tBx_EponFPGA.Text = sline[4];
                       //sr.Close();
                    //}
                    //if (line.Contains("GPON"))       //GPON
                    //{
                        //string s = sr.ReadLine();  这句是读取当前行的下一行                          
                    //    string[] sline = line.Trim().Split(' ');    //
                        tBx_GponSoftware.Text = sline[7];
                        tBx_GponFPGA.Text = sline[9];
                         
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                //MessageBox.Show("404");
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        //Ping
        private void btnPingDevice_Click(object sender, EventArgs e)
        {
            //save config info
            expressions.Add("EPON" + " Software: " + tBx_EponSoftware.Text.ToString() + " FPGA: " + tBx_EponFPGA.Text.ToString() 
                + " GPON" + " Software: " + tBx_GponSoftware.Text.ToString() + " FPGA: " + tBx_GponFPGA.Text.ToString() +" "+ DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));

            Thread.Sleep(10);
            writeInfo();

            ShowStatusBar(string.Empty, true);

            string ipAddress = tbxDeviceIP.Text.Trim();

            bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
            if (!isValidIpA)
                throw new Exception("The Device IP is invalid !!");

            isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
            if (isValidIpA)
                ShowStatusBar("The device is active", true);
            else
                ShowStatusBar("Could not read any response", false);

        }


        #region    btn about & btn view & label time
        private ABOUT AboutForm = new ABOUT();
        private void button1_Click(object sender, EventArgs e)
        {
            AboutForm.ShowInTaskbar = false;
            AboutForm.ShowDialog();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                string LogPath = System.Environment.CurrentDirectory + "//log.txt";
                System.Diagnostics.Process.Start(LogPath);
            }
            catch {
                MessageBox.Show("未找到 log.txt");
            }
        }

        //time label
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.lblTime.Text = DateTime.Now.ToString();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //log path
            tbxView.Text = System.Environment.CurrentDirectory + "\\log.txt";

            //Timer tick
            System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(timer1_Tick);
            myTimer.Enabled = true;
            myTimer.Interval = 1000;
            myTimer.Start();
        }
    }
}
