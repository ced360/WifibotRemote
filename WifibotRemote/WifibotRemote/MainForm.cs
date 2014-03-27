#define SIMU
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Media;


namespace WifibotRemote
{
    public partial class MainForm : Form
    {
        ////////////////
        /// Variables
        ///////////////
        volatile bool simu = false;
        Com com;
        bool _connected = false;
        volatile List<String> keys=new List<String>();
        System.Threading.Thread ComThread;
        Thread keyThread = null;
        bool _playingvideo = false;
        ////////////////
        /// Acces externe et variables paramettrées
        ///////////////
        //button
        public delegate bool buttonVisibleDelagate(bool val);
        public bool buttonVisible
        {
            get
            {
                buttonVisibleDelagate del = new buttonVisibleDelagate(GetButton1Value);
                if (!InvokeRequired)
                {
                    // return buttonConnect.Enabled;
                    return true;
                }
                else
                {
                    //return (bool)Invoke(del, new bool() { val });
                    return (bool)Invoke(del, new bool());
                }
            }
            set
            {
                if (!InvokeRequired)
                {
                    // buttonConnect.Enabled = value;
                }
                else
                {
                    Invoke(new Action(() => buttonVisible = value));
                    //Invoke(new Action<int>(UpdateProgress), percentComplete);
                }

            }
        }
        private bool GetButton1Value(bool b)
        {
            return true;//return buttonConnect.Enabled;
        }
        //connected
        public delegate bool connectedDelagate(bool val);
        public bool connected
        {
            get
            {
                connectedDelagate del = new connectedDelagate(GetConnectedValue);
                if (!InvokeRequired)
                {
                    return _connected;
                }
                else
                {
                    //return (bool)Invoke(del, new bool() { val });
                    return (bool)Invoke(del, new bool());
                }
            }
            set
            {
                if (!InvokeRequired)
                {

                    if (value)
                    {
                        //menu.Items["Connection"].Text = "Déconnecter";
                        
                        ComThread = new System.Threading.Thread(() => com.Connect("192.168.1.106", 15020));
                        ComThread.Start();
                        while (ComThread.IsAlive)
                        {
                            Thread.Sleep(1);
                        }
                        if (com.socketConnected())
                        {
                            _connected = true;
                            log("Connected to " + "192.168.1.96");
                        }
                        else
                        {
                            MessageBox.Show("Erreur de connection");
                        }
                        //webBrowser.GoHome();
                    }
                    else
                    {
                        //menu.Items[1].Text = "Connecter";
                        
                        _connected = false;
                        //while (ComThread.IsAlive)
                        //{ Thread.Sleep(10); }
                        (new System.Threading.Thread(() => com.Disconnect())).Start();
                        Thread.Sleep(10);
                        
                        log("Disconnected from server");
                        
                    }

                    //buttonDisconnect.Enabled = value;
                    //buttonConnect.Enabled = !value;
                }
                else
                {
                    Invoke(new Action(() => _connected = value));
                    //Invoke(new Action<int>(UpdateProgress), percentComplete);
                }

            }
        }
        private bool GetConnectedValue(bool b)
        {
            return _connected;
        }
        //labelLog
        public void log(string val)
        {
            if (!InvokeRequired)
            {
                labelLog.Text = val;
            }
            else
            {
                Invoke(new Action(() => log(val)));
            }

        }
        //MessageBox
        public void msgbox(string val)
        {
            if (!InvokeRequired)
            {
                MessageBox.Show(val);
            }
            else
            {
                Invoke(new Action(() => msgbox(val)));
            }

        }
        ////////////////
        /// General
        ///////////////
        public MainForm()
        {
            InitializeComponent();
            com = new Com(this);
            this.KeyPreview = true;
            //Condition initial
            //buttonDisconnect.Enabled = false;
            keyThread= new System.Threading.Thread(() => keyProg());
            keyThread.Start();

        }

        ////////////////
        /// Evenements
        ///////////////
        //Clavier
        private void keyProg()
        { 
            byte[] msg;
            if (simu)
            {
                msg = new byte[2];
                while (true)
                {

                    if (connected)
                    {
                        msg[0] = 0;
                        msg[1] = 0;
                        if (keys.Contains("Z"))
                        {
                            msg[0] += 128;
                            msg[1] += 128;
                            msg[0] += 64;
                            msg[1] += 64;
                            msg[0] += 16;
                            msg[1] += 16;

                        }
                        else if (keys.Contains("S"))
                        {
                            msg[0] += 128;
                            msg[1] += 128;
                            msg[0] += 16;
                            msg[1] += 16;
                        }

                        if (keys.Contains("Q"))
                        {
                            msg[1] += 16;
                        }

                        if (keys.Contains("D"))
                        {
                            msg[0] += 16;
                        }
                        if ((keys.Contains("Z")) || (keys.Contains("S")) || (keys.Contains("D")) || (keys.Contains("Q")))
                        {
                            (new System.Threading.Thread(() => com.Send(msg))).Start();
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            else
            {
                uint crc16 = 0;
                msg = new byte[9];
                byte[] crcmsg = new byte[6];
                
                while (true)
                {
                    
                    if (connected)
                    {
                        msg[0] = 255;
                        msg[1] = 0x07;
                        msg[2] = 0;
                        msg[3] = 0;
                        msg[4] = 0;
                        msg[5] = 0;
                        msg[6] = 0; //Convert.ToByte("11110000", 2);
                        msg[7] = 0;
                        msg[8] = 0;

                        if (keys.Contains("Z"))
                        {
                            msg[2] = 120;
                            msg[4] = 120;
                            msg[6] += 128+64+32+16;
                        }
                        else if (keys.Contains("S"))
                        {
                            msg[2] = 120;
                            msg[4] = 120;
                            msg[6] += 128 + 32;
                        }

                        if (keys.Contains("Q"))
                        {
                            msg[2] += 60;
                            msg[4] -= 60;
                        }

                        if (keys.Contains("D"))
                        {
                            msg[2] -= 60;
                            msg[4] += 60;
                        }
                        crcmsg[0] = msg[1];
                        crcmsg[1] = msg[2];
                        crcmsg[2] = msg[3];
                        crcmsg[3] = msg[4];
                        crcmsg[4] = msg[5];
                        crcmsg[5] = msg[6];
                        crc16=Crc16(crcmsg);

                        msg[8] = (byte)(crc16 >> 8);
                        msg[7] = (byte)(crc16 );

                        foreach (byte b in msg)
                        {
                            Console.Write((int)b + " ");
                        }
                        Console.WriteLine();
                        //if ((keys.Contains("Z")) || (keys.Contains("S")) || (keys.Contains("D")) || (keys.Contains("Q")))
                        //{
                        while (ComThread.IsAlive)
                        { Thread.Sleep(10); }

                        com.Send(msg);
                        com.Receive();

                        //ComThread = new System.Threading.Thread(() => com.Send(msg));
                        //ComThread.Start();
                            //(new System.Threading.Thread(() => com.Send(msg))).Start();
                        //}
                    }
                    Thread.Sleep(50);
                }
            }
            
        }

        public uint Crc16(byte[] data)
        {
            uint Crc = 0xFFFF;
            uint Polynome = 0xA001;
            uint CptOctet = 0;
            uint CptBit = 0;
            uint Parity = 0;

            Crc = 0xFFFF;
            Polynome = 0xA001;
            for (CptOctet = 0; CptOctet < 6; CptOctet++)
            {
                Crc ^= data[CptOctet];

                for (CptBit = 0; CptBit <= 7; CptBit++)
                {
                    Parity = Crc;
                    Crc >>= 1;
                    if (Parity % 2 == 1)
                        Crc ^= Polynome;
                }
            }
            return (Crc);

        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyCode.ToString());
            if (!keys.Contains(e.KeyCode.ToString()))
            { keys.Add(e.KeyCode.ToString()); }

            byte[] msg = new byte[2];
            msg[0]=0;
            msg[1]=0;

            Console.WriteLine(e.KeyCode.ToString());
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (keys.Contains(e.KeyCode.ToString()))
            { keys.Remove(e.KeyCode.ToString()); }
        }

        //Menu
        private void connecterdeconnecterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                connected = true;
            }
            else
            {
                connected = false;
            }
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            keyThread.Abort();

            if (connected)
            {
                (new System.Threading.Thread(() => com.Disconnect())).Start();
                log("Disconnected from server");
            }
        }

        private void afficherVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //axVLCPlugin21.Hide();

            axVLCPlugin21.FullscreenEnabled = true;
            var uri = new Uri(@"http://192.168.1.106:8080/?action=stream");
            var converted = uri.AbsoluteUri;
            axVLCPlugin21.playlist.add(converted);
            axVLCPlugin21.playlist.play();

        }

    }
}
