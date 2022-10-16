using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SonyMDRemote
{
    public partial class SonyMDRemote : Form
    {
        public SonyMDRemote()
        {
            InitializeComponent();

            label1.Text = String.Format("LA2YUA SonyMDRemote {0} {1}", VersionString, ReleaseString);
#if LOGGING
            string logfilename = String.Format("Log_{0}.txt", DateTime.UtcNow.ToString("o").Replace(':', '_'));
            logfile = new StreamWriter(logfilename, append: true);
            if (logfile == null)
                richTextBox_Log.AppendText("Error opening log file!\r\n");
            else
                richTextBox_Log.AppendText(String.Format("Log file: {1}\\{0}\r\n", logfilename, Directory.GetCurrentDirectory()));

            string logfile_allname = String.Format("Log_{0}_data.txt", DateTime.UtcNow.ToString("o").Replace(':', '_'));
            logfile_cmd = new StreamWriter(logfile_allname, append: true);
            if (logfile_cmd == null)
                richTextBox_Log.AppendText("Error opening command log file!\r\n");
            else
                richTextBox_Log.AppendText(String.Format("Command log file: {1}\\{0}\r\n", logfile_allname, Directory.GetCurrentDirectory()));

            logfile.AutoFlush = true;
            logfile_cmd.AutoFlush = true;
#endif
            AppendLog("Program start version {0} {1}", VersionString, ReleaseString);
            AppendCmdLog("Program start version {0} {1}", VersionString, ReleaseString);
            AppendLog("Hint: select a COM port and hit Get Info to start everything");

#if LOGGING
            AppendLog("Debug build, unlimited scrollback and logging.");
#elif !LOGGING
            //AppendLog("Hint: select a COM port and hit Get Info to start everything");
#endif

            Update_COM_List(true);
            comboBox1.SelectedIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(comboBox1_SelectedIndexChanged);

#if !LOGGING
            button17.Visible = false;
#endif
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox ch = (ComboBox)sender;
            switch (ch.SelectedIndex)
            {
                case 0: Transmit_MDS_Message(MDS_TX_RepeatOff); break;
                case 1: Transmit_MDS_Message(MDS_TX_RepeatAll); break;
                case 2: Transmit_MDS_Message(MDS_TX_Repeat1Tr); break;
            }
            
        }

        string VersionString = "v0.3a";

#if LOGGING
        string ReleaseString = "debug build";
#elif !LOGGING
        string ReleaseString = "release build";
#endif

        StreamWriter logfile;
        StreamWriter logfile_cmd;

        UInt32 loglines = 0;
#if LOGGING
        UInt32 loglineslimit = 10000;
#elif !LOGGING
        UInt32 loglineslimit = 200;
#endif
        private void AppendLog(string s, params object[] format)
        {
#if LOGGING
            if (logfile == null)
            {
                return;
            }
#endif
            string datestamp = String.Format("{0}Z: ", DateTime.UtcNow.ToString("s"));
            string datestamp_log = String.Format("{0}Z: ", DateTime.UtcNow.ToString("o"));
            string data = String.Format("{0}\r\n", String.Format(s, format));
            richTextBox_Log.AppendText(datestamp);
            richTextBox_Log.AppendText(data);
            loglines++;
            
            // limit the scrollback
            if ((loglines > loglineslimit) && (loglines % loglineslimit == 0))
            {
                var lines = richTextBox_Log.Lines;
                richTextBox_Log.Lines = lines.Skip((int)(lines.Length - loglineslimit)).ToArray();
            }

#if LOGGING
            logfile.Write(datestamp_log + data);
#endif

            AppendCmdLog(data);
        }

        private void AppendCmdLog(string s, params object[] format)
        {
#if LOGGING
            if (logfile_cmd == null)
            {
                return;
            }
            string datestamp = String.Format("{0}Z: ", DateTime.UtcNow.ToString("o"));
            string data = String.Format("{0}\r\n", String.Format(s, format));
            logfile_cmd.Write(datestamp + data);
#endif
        }

        private void Update_COM_List(bool startup = false)
        {
            // 1 Hz timer
            // check if new serial ports were added or removed

            //ComboBox.ObjectCollection old_serial_list = comboBox1_Serial_Port.Items;
            string[] SerialPorts = SerialPort.GetPortNames();
            bool list_outdated = false;
            foreach (string curport in SerialPorts)
            {
                if (!comboBox1_Serial_Port.Items.Contains(curport))
                {
                    list_outdated = true;
                }
            }

            if (comboBox1_Serial_Port.Items.Count != SerialPorts.Length)
            {
                list_outdated = true;
            }

            if (list_outdated)
            {
                // Log serial port changes
                StringBuilder sb = new StringBuilder();
                sb.Append("Serial ports changed:");
                List<string> oldserialports = new List<string>();

                foreach (var s in comboBox1_Serial_Port.Items)
                {
                    oldserialports.Append(s.ToString());
                    if (!SerialPorts.Contains(s.ToString()))
                    {
                        sb.AppendFormat(" -{0}", s.ToString());
                    }
                }

                foreach (string s in SerialPorts)
                {
                    if (!oldserialports.Contains(s))
                    {
                        sb.AppendFormat(" +{0}", s.ToString());
                    }
                }

                AppendLog(sb.ToString());

                string old_selection = String.Empty;

                if (comboBox1_Serial_Port.Items.Count > 0)
                {
                    old_selection = comboBox1_Serial_Port.Items[comboBox1_Serial_Port.SelectedIndex].ToString();
                }

                int new_selection = -1;
                // populate serial port list
                comboBox1_Serial_Port.Items.Clear();
                foreach (string curport in SerialPort.GetPortNames())
                {
                    comboBox1_Serial_Port.Items.Add(curport);
                    if (old_selection == curport && !String.IsNullOrEmpty(old_selection))
                    {
                        new_selection = comboBox1_Serial_Port.Items.Count - 1;
                    }
                }


                if (comboBox1_Serial_Port.Items.Count > 0)
                {
                    string stored_port = (string)Properties.Settings.Default["Selected_COM_Port"];
                    if (comboBox1_Serial_Port.Items.Contains(stored_port) && startup)
                    {
                        comboBox1_Serial_Port.SelectedIndex = comboBox1_Serial_Port.Items.IndexOf(stored_port);
                        AppendLog("Selected last used serial port {0}", stored_port);
                    }
                    else if (new_selection < 0)
                    {
                        AppendLog("Previously selected port not present, now using {0}", comboBox1_Serial_Port.Items[0].ToString());
                        // select index 0
                        comboBox1_Serial_Port.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox1_Serial_Port.SelectedIndex = new_selection;
                    }

                }
            }
        }

        private void timer1_Maintenance_Tick(object sender, EventArgs e)
        {
            if (checkBox1_Autopoll.Checked)
            {
                timer1_Maintenance.Interval = 5000;
                DoUpdateTask();
            }
            else
            {
                timer1_Maintenance.Interval = 10000;
            }
            Update_COM_List();


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //System.Threading.Thread.Sleep(100);
            if (serialPort1.IsOpen)
            {
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
                serialPort1.Close();
            }
#if LOGGING
            logfile.Flush();
#endif
            if (comboBox1_Serial_Port.Text != String.Empty)
            {
                Properties.Settings.Default["Selected_COM_Port"] = comboBox1_Serial_Port.Text;
            }
            Properties.Settings.Default.Save();
        }

        private bool Try_Open_Serial_Port()
        {
            if (serialPort1.IsOpen)
            {
                checkBox1_Autopoll.Checked = false;
                commandqueue.Clear();
                commandqueue_priority.Clear();
                serialPort1.Close();
                AppendLog("Closing port {0}", serialPort1.PortName);
                button_Serial_Connect.Text = "Connect";
                button_Serial_Connect.BackColor = default(Color);
                button_Serial_Connect.UseVisualStyleBackColor = true;
                //button_Poll.BackColor = default(Color);
                //button_Poll.UseVisualStyleBackColor = true;
                //timer_Serial_Poll.Stop();
                return false;
            }

            if (comboBox1_Serial_Port.SelectedItem == null)
            {
                AppendLog("Please select a COM port first.");
                return false;
            }

            // Try to open COM port
            try
            {
                serialPort1.PortName = comboBox1_Serial_Port.SelectedItem.ToString();

                AppendLog("Opening {0}, {1}-{2}{3}{4}",
                    serialPort1.PortName,
                    serialPort1.BaudRate,
                    serialPort1.DataBits,
                    serialPort1.Parity.ToString().Substring(0, 1),
                    Convert.ToInt32(serialPort1.StopBits));
                serialPort1.Open();
            }
            catch (InvalidOperationException)
            {
                // port already open
                return true;
            }
            catch (Exception exc)
            {
                AppendLog("Error opening serial port: " + exc.Message);
                return false;
            }
            AppendLog("Port opened");

            button_Serial_Connect.BackColor = Color.Orange;
            button_Serial_Connect.Text = "Disconnect";

            //if (checkBox_AutoPoll.Checked)
                //timer_Serial_Poll.Start();
            return true;
        }

        private void button_Serial_Connect_Click(object sender, EventArgs e)
        {
            Try_Open_Serial_Port();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                int size = sp.BytesToRead;
                byte[] rxdata = new byte[size];
                sp.Read(rxdata, 0, size);
                serialData_input(rxdata);
            }
            catch (Exception serialexception)
            {
                AppendLog("Serial port error {0}", serialexception.Message);
            }
            //backgroundWorker1.RunWorkerAsync(rxdata);
        }

        public bool ByteEquality(byte[] a1, byte[] b1)
        {
            int i;
            if (a1.Length == b1.Length)
            {
                i = 0;
                while (i < a1.Length && (a1[i] == b1[i])) //Earlier it was a1[i]!=b1[i]
                {
                    i++;
                }
                if (i == a1.Length)
                {
                    return true;
                }
            }

            return false;
        }

        byte[] MDS_TX_SetRemoteOn = new byte[] { 0x10, 0x03 };
        byte[] MDS_TX_SetRemoteOff = new byte[] { 0x10, 0x04 };

        byte[] MDS_TX_ReqStatus = new byte[] { 0x20, 0x20 };
        byte[] MDS_TX_ReqDiscData = new byte[] { 0x20, 0x21 };
        byte[] MDS_TX_ReqModelName = new byte[] { 0x20, 0x22 };
        byte[] MDS_TX_ReqTrackRecordDate = new byte[] { 0x20, 0x24}; // next byte is track number
        byte[] MDS_TX_ReqTOCData = new byte[] { 0x20, 0x44, 0x01 };
        byte[] MDS_TX_ReqTrackTime = new byte[] { 0x20, 0x45, 0x01 }; // next byte is track number
        byte[] MDS_TX_ReqDiscName = new byte[] { 0x20, 0x48, 0x01 };
        byte[] MDS_TX_ReqTrackName = new byte[] { 0x20, 0x4A }; // next byte is track number
        byte[] MDS_TX_ReqDiscAndTrackNames = new byte[] { 0x20, 0x4C, 0x01 };
        byte[] MDS_TX_ReqRemainingRecordTime = new byte[] { 0x20, 0x54, 0x01 };
        byte[] MDS_TX_ReqTrackRemainingNameSize = new byte[] { 0x20, 0x55, 0x00 }; // next byte is track number

        // 6.42 DISC NAME WRITE
        // Playback must be stopped first
        // supports ASCII values 0, 0x20-0x5A, 0x5E-0x7A
        // should wait for WRITE PACKET RECEIVED message between writes
        // final namedata byte must be 0 to effect write
        // then finally WRITE COMPLETE?
        byte[] MDS_TX_WriteDiscName1 = new byte[] { 0x20, 0x70 }; // next byte is packet number, next 16 bytes is name data
        byte[] MDS_TX_WriteDiscName2 = new byte[] { 0x20, 0x71}; // next byte is packet number, next 16 bytes is name data, null terminated

        // 6.43 TRACK NO. NAME WRITE
        // Playback must be stopped first
        // supports ASCII values 0, 0x20-0x5A, 0x5E-0x7A
        // should wait for WRITE PACKET RECEIVED message between writes
        // final namedata byte must be 0 to effect write
        // then finally WRITE COMPLETE
        byte[] MDS_TX_WriteTrackName1 = new byte[] { 0x20, 0x72 }; // next byte is track number, next 16 bytes is name data, null terminated
        byte[] MDS_TX_WriteTrackName2 = new byte[] { 0x20, 0x73 }; // next byte is packet number, next 16 bytes is name data, null terminated

        // basic transport controls
        byte[] MDS_TX_Play = new byte[] { 0x02, 0x01 };
        byte[] MDS_TX_Stop = new byte[] { 0x02, 0x02 };
        byte[] MDS_TX_PlayPause = new byte[] { 0x02, 0x03 };
        byte[] MDS_TX_Pause = new byte[] { 0x02, 0x06 };
        byte[] MDS_TX_FFW_REW_OFF = new byte[] { 0x00 };
        byte[] MDS_TX_Rewind = new byte[] { 0x02, 0x13 };
        byte[] MDS_TX_FastForward = new byte[] { 0x02, 0x14 };
        byte[] MDS_TX_NextTrack = new byte[] { 0x02, 0x16 };
        byte[] MDS_TX_PrevTrack = new byte[] { 0x02, 0x15 };
        byte[] MDS_TX_RecordArm = new byte[] { 0x02, 0x21 };
        byte[] MDS_TX_Eject = new byte[] { 0x02, 0x40 };
        byte[] MDS_TX_AutoPauseOn = new byte[] { 0x02, 0x81 };
        byte[] MDS_TX_AutoPauseOff = new byte[] { 0x02, 0x80 };

        // from addendum
        byte[] MDS_TX_RepeatOff = new byte[] { 0x02, 0xA0 };
        byte[] MDS_TX_RepeatAll = new byte[] { 0x02, 0xA1 };
        byte[] MDS_TX_Repeat1Tr = new byte[] { 0x02, 0xA2 };

        // track management
        byte[] MDS_TX_StartPlayAtTrack = new byte[] { 0x03, 0x42, 0x01 }; // next byte is track number
        byte[] MDS_TX_PausePlayAtTrack = new byte[] { 0x03, 0x43, 0x01 }; // next byte is track number, pauses at the start of specific track

        byte[] MDS_TX_EnableElapsedTimeTransmit = new byte[] { 0x07, 0x10 };
        byte[] MDS_TX_DisableElapsedTimeTransmit = new byte[] { 0x07, 0x11 };

        // issue a null-payload if payload is even multiple of 16

        // command queue object, list of raw commands to issue and what delay should be used before the next command
        List<Tuple<byte[], int>> commandqueue = new List<Tuple<byte[], int>>();
        List<Tuple<byte[], int>> commandqueue_priority = new List<Tuple<byte[], int>>();

        private void Transmit_MDS_Write(string name, byte tracknumber = 0)
        {
            if (!serialPort1.IsOpen)
            {
                return;
            }

            timer_Poll_Time.Stop();

            List<byte> txname = new List<byte>(name.Length);
            txname.AddRange(Encoding.ASCII.GetBytes(name));
            txname.Add(0);

            bool first = true;
            int count = 0;
            // work out how many iterations are needed
            while(txname.Count > 0)
            {
                count++;
                List<byte> txdata = new List<byte>(32);
                // header, 0x7E is PC to MD, 0x6E is MD to PC
                txdata.Add(0x7E);
                // length from header to terminator, up to 32 (0x20)
                // computed at the end of the function
                txdata.Add(0x00);
                // format type, fixed
                txdata.Add(0x05);
                // category, fixed
                txdata.Add(0x47);

                // add command and sequence number if required
                if (first && tracknumber == 0)
                {
                    txdata.AddRange(MDS_TX_WriteDiscName1);
                    first = false;
                    txdata.Add((byte)count);
                }
                else if (!first && tracknumber == 0)
                {
                    txdata.AddRange(MDS_TX_WriteDiscName2);
                    txdata.Add((byte)count);
                }
                else if (first)
                {
                    txdata.AddRange(MDS_TX_WriteTrackName1);
                    first = false;
                    txdata.Add((byte)tracknumber);
                }
                else if (!first)
                {
                    txdata.AddRange(MDS_TX_WriteTrackName2);
                    txdata.Add((byte)count);
                }

                

                int payloadcount = 0;
                // iterate over data until we reach the limit
                foreach (char s in txname)
                {
                    byte ss = (byte)s;
                    // remove illegal values
                    if (s != 0 && (s < 0x20 || (s > 0x5A && s < 0x5E) || s > 0x7A))
                        ss = (byte)' ';

                    payloadcount++;
                    txdata.Add(ss);
                    if (payloadcount == 16)
                        break;
                }
                txname.RemoveRange(0, payloadcount);

                // terminator
                txdata.Add(0xFF);

                txdata[1] = (byte)txdata.Count;

                if (txdata.Count > 32)
                    throw new ArgumentException("Transmission packet would be too big!");
                var tup = new Tuple<byte[], int>(txdata.ToArray(), 500);
                // ignore by default if payload is already present
                commandqueue_priority.Add(tup);
            }

            timer_Poll_Time.Start();

            timer_Poll_Time.Enabled = true;
        }

        private void Transmit_MDS_Message(byte[] data, int delay = 300, byte tracknumber = 0, bool allowduplicates = false, bool priority = false)
        {
            if (!serialPort1.IsOpen)
            {
                //return;
                if (!Try_Open_Serial_Port())
                {
                    return;
                }
            }
            List<byte> txdata = new List<byte>(20);
            // header, 0x7E is PC to MD, 0x6E is MD to PC
            txdata.Add(0x7E);
            // length from header to terminator, up to 32 (0x20)
            // computed at the end of the function
            txdata.Add(0x00);
            // format type, fixed
            txdata.Add(0x05);
            // category, fixed
            txdata.Add(0x47);


            // data
            txdata.AddRange(data);
            if (tracknumber > 0)
                txdata.Add(tracknumber);

            // terminator
            txdata.Add(0xFF);

            txdata[1] = (byte)txdata.Count;

            if (txdata.Count > 32)
                throw new ArgumentException("Transmission packet would be too big!");

            var tup = new Tuple<byte[], int>(txdata.ToArray(), delay);
            // ignore by default if payload is already present
            // but we insert the requested command at the end of the sequence, removing the previous command
            if (!allowduplicates)
            {
                var foundcmd = commandqueue.Find(item => ByteEquality(item.Item1, tup.Item1));
                if (foundcmd != null)
                    commandqueue.Remove(foundcmd);
            }

            if (!priority)
                commandqueue.Add(tup);
            else
                commandqueue.Insert(0, tup);
            timer_Poll_Time.Enabled = true;
            //serialPort1.Write(txdata.ToArray(), 0, txdata.Count);

            //Start_Timeout();
        }

        private void timer_Poll_Time_Tick(object sender, EventArgs e)
        {
            Tuple<byte[], int> nextcommand;

            // do priority commands first
            if (commandqueue_priority.Count > 0)
            {
                // pop off the first item, transmit, then reset the timer with the delay
                nextcommand  = commandqueue_priority[0];
                commandqueue_priority.RemoveAt(0);

                AppendLog("PC sent priority: {0}, ASCII: {1}", BitConverter.ToString(nextcommand.Item1), TrimNonAscii(System.Text.Encoding.ASCII.GetString(nextcommand.Item1)));

                serialPort1.Write(nextcommand.Item1, 0, nextcommand.Item1.Length);

                timer_Poll_Time.Interval = nextcommand.Item2;
                return;
            }

            // empty queue
            if (commandqueue == null || commandqueue.Count < 1)
            {
                timer_Poll_Time.Enabled = false;
                return;
            }
            // just spin and don't issue new command if we're getting the disc data
            else if (_inforequest && timer_Poll_GetInfo.Enabled)
            {
                timer_Poll_Time.Interval = 2000;
                return;
            }
                

            // pop off the first item, transmit, then reset the timer with the delay
            nextcommand = commandqueue[0];
            commandqueue.RemoveAt(0);
#if DEBUG
            AppendLog("PC sent: {0}, ASCII: {1}", BitConverter.ToString(nextcommand.Item1), TrimNonAscii(System.Text.Encoding.ASCII.GetString(nextcommand.Item1)));
#endif
            serialPort1.Write(nextcommand.Item1, 0, nextcommand.Item1.Length);

            timer_Poll_Time.Interval = nextcommand.Item2;
        }

        enum serialRXState
        {
            serialRXState_Idle,
            serialRXState_Started,
            serialRXState_Escape,
            serialRxState_Stop
        };

        // 7.11 STATUS DATA
        enum MDS_Status_D1
        {
            STOP = 0,
            PLAY = 1,
            PAUSE = 2,
            EJECT = 3,
            REC_PLAY = 4,
            REC_PAUSE = 5,
            rehearsal = 6,
            reserved = 7
        };

        // 7.11 STATUS DATA addendum
        enum MDS_Status_D2_Repeat
        {
            REPEAT_OFF,
            ALL_REPEAT,
            TRACK_REPEAT
        };

        // 7.11 STATUS DATA
        enum MDS_Status_D3_Source
        {
            Analog = 1,
            Optical = 3,
            Coaxial = 5,
            reserved = 7
        };

        bool IsBitSet(byte b, int pos)
        {
            return ((b >> pos) & 1) != 0;
        }

        private string TrimNonAscii(string value)
        {
            string pattern = "[^ -~]*";
            Regex reg_exp = new Regex(pattern);
            return reg_exp.Replace(value, "");
        }

        // our received strings are either null or FF terminated depending on where in the sequence they are
        // so we need to handle both
        private string DecodeAscii(ref byte[] buffer, int start)
        {
            int count = Array.IndexOf<byte>(buffer, 0, start) - start;
            if (count < 0) count = Array.IndexOf<byte>(buffer, 0xff, start) - start;

            if (count < 0) count = (buffer.Length - start) - 1;
            return Encoding.ASCII.GetString(buffer, start, count);
        }

        private serialRXState receiverstate = serialRXState.serialRXState_Idle;
        private List<byte> serialRXData = new List<byte>();
        delegate void SetSerialDataInputCallback(byte[] text);

        byte _currentrack = 1;
        byte _packetlen = 0;
        int _infocounter = -1;
        bool toc_read_done = false;
        //byte _currtracklen_min = 0;
        //byte _currtracklen_sec = 0;
        byte _lasttrackno = 0;
        TimeSpan _remainingrecordtime = new TimeSpan(0);
        TimeSpan _disclength = new TimeSpan(0);
        string discname;

        // list of track names
        IDictionary<int, StringBuilder> tracknames = new Dictionary<int, StringBuilder>();

        // list of track lengths
        IDictionary<int, TimeSpan> tracklengths = new Dictionary<int, TimeSpan>();

        private void serialData_input(byte[] text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.richTextBox_Log.InvokeRequired)
            {
                SetSerialDataInputCallback d = new SetSerialDataInputCallback(serialData_input);
                this.BeginInvoke(d, new object[] { text });
                return;
            }

            byte[] rxdata = text;

            foreach (byte currentchar in rxdata)
            {
                serialRXData.Add(currentchar);
                // receiving start byte
                if (receiverstate != serialRXState.serialRXState_Started && currentchar == (char)0x6F)
                {
                    receiverstate = serialRXState.serialRXState_Started;
                    //AppendLog("Start byte, clearing old trash:", BitConverter.ToString(serialRXData.ToArray()));
                    serialRXData.Clear();
                    serialRXData.Add(0x6F);
                    //AppendLog("Header received");
                }
                // receiving payload data length
                else if (receiverstate == serialRXState.serialRXState_Started && serialRXData.Count == 2)
                {
                    // max length is 0x20
                    if (currentchar <= 0x20)
                        _packetlen = currentchar;
                }
                // end of transmission
                else if (receiverstate == serialRXState.serialRXState_Started && serialRXData.Count == _packetlen)
                {
                    receiverstate = serialRXState.serialRxState_Stop;
                    if (currentchar != 0xff)
                    {
                        AppendLog("End of packet but terminator was {0:X}", currentchar);
                    }
                    //serialRXData.Add(currentchar);
                    receiverstate = serialRXState.serialRxState_Stop;


                    //timer_Serial_Timeout.Stop();
                    byte[] ArrRep = serialRXData.ToArray();

                    //serialRXData.Clear();
#if DEBUG
                    AppendLog("MD sent: {0}, ASCII: {1}", BitConverter.ToString(ArrRep), TrimNonAscii(System.Text.Encoding.ASCII.GetString(ArrRep)));
#endif
                    if (ArrRep.Length < 5)
                        break;

                    // 7.2 REMOTE MODE
                    if (ArrRep[4] == 0x10)
                    {
                        AppendLog("MD: Remote is {0}", ArrRep[5] == 0x03 ? "on" : "off");
                    }

                    // 7.6 PAUSE
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x03)
                    {
                        AppendLog("MD: Paused");
                    }

                    // 7.5 STOP
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x02)
                    {
                        AppendLog("MD: Stopped");
                    }

                    // 7.4 PLAY
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x01)
                    {
                        AppendLog("MD: Playing");
                    }

                    // 7.7 REC
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x21)
                    {
                        AppendLog("MD: Recording started");
                    }

                    // 7.8 REC PAUSE
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x25)
                    {
                        AppendLog("MD: Recording paused");
                    }

                    // 7.9 EJECT
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x40)
                    {
                        AppendLog("MD: Disc ejected");
                    }

                    // 7.10 MODEL DATA
                    if (ArrRep[4] == 0x02 && ArrRep[5] == 0x61)
                    {
                        if (ArrRep[6] == 0x03)
                            AppendLog("MD: recording & time machine recording capable");
                        else
                            AppendLog("MD: info received for unknown model");
                    }

                    // 7.11 STATUS DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x20)
                    {
                        byte Data1 = ArrRep[6];
                        byte Data2 = ArrRep[7];
                        byte Data3 = ArrRep[8];
                        // 9 is fixed 1
                        byte TrackNo = ArrRep[10];
                        ReceivedPlayingTrack(TrackNo);

                        // these two seems to be inverted or useless on the E12
                        bool nodiscinserted = IsBitSet(Data1, 5);
                        bool poweredoff = IsBitSet(Data1, 4);

                        MDS_Status_D1 playbackstatus = MDS_Status_D1.reserved;
                        string playbackstatusstr = "Unknown";
                        switch (Data1 & 0x0f)
                        {
                            case 0x0:
                                playbackstatus = MDS_Status_D1.STOP;
                                playbackstatusstr = "Stop";
                                break;
                            case 0x1:
                                playbackstatus = MDS_Status_D1.PLAY;
                                playbackstatusstr = "Play";
                                break;
                            case 0x2:
                                playbackstatus = MDS_Status_D1.PAUSE;
                                playbackstatusstr = "Pause";
                                break;
                            case 0x3:
                                playbackstatus = MDS_Status_D1.EJECT;
                                playbackstatusstr = "Eject";
                                break;
                            case 0x4:
                                playbackstatus = MDS_Status_D1.REC_PLAY;
                                playbackstatusstr = "Rec. Play";
                                break;
                            case 0x5:
                                playbackstatus = MDS_Status_D1.REC_PAUSE;
                                playbackstatusstr = "Rec. Pause";
                                break;
                            case 0x6:
                                playbackstatus = MDS_Status_D1.rehearsal;
                                playbackstatusstr = "rehearsal";
                                break;

                        }

                        label3.Text = playbackstatusstr;

                        toc_read_done = IsBitSet(Data2, 7);
                        bool rec_possible = IsBitSet(Data2, 5);

                        MDS_Status_D2_Repeat repeatstatus = MDS_Status_D2_Repeat.REPEAT_OFF;

                        if (IsBitSet(Data2, 4) && !IsBitSet(Data2, 3))
                            repeatstatus = MDS_Status_D2_Repeat.TRACK_REPEAT;
                        else if (!IsBitSet(Data2, 4) && IsBitSet(Data2, 3))
                            repeatstatus = MDS_Status_D2_Repeat.ALL_REPEAT;
                        else if (!IsBitSet(Data2, 4) && !IsBitSet(Data2, 3))
                            repeatstatus = MDS_Status_D2_Repeat.REPEAT_OFF;

                        string repeatstr = "";
                        switch (repeatstatus)
                        {
                            case MDS_Status_D2_Repeat.ALL_REPEAT: repeatstr = "Repeat"; break;
                            case MDS_Status_D2_Repeat.TRACK_REPEAT: repeatstr = "1Tr Repeat"; break;
                            case MDS_Status_D2_Repeat.REPEAT_OFF: repeatstr = "No Repeat"; break;
                        }

                        label11.Text = repeatstr;

                        bool mono = IsBitSet(Data3, 7);
                        bool copy_protected = IsBitSet(Data3, 6);
                        bool digital_in_unlocked = IsBitSet(Data3, 5);

                        MDS_Status_D3_Source recsource = MDS_Status_D3_Source.reserved;
                        string recsourcestr = "Unknown";
                        switch (Data3 & 0x07)
                        {
                            case 0x1:
                                recsource = MDS_Status_D3_Source.Analog;
                                recsourcestr = "Analog";
                                break;
                            case 0x3:
                                recsource = MDS_Status_D3_Source.Optical;
                                recsourcestr = "Optical";
                                break;
                            case 0x5:
                                recsource = MDS_Status_D3_Source.Coaxial;
                                recsourcestr = "Coaxial";
                                break;

                        }

                        AppendLog("MD: Status dump {0} {1} {9} track no. {2} {3} {4} {5} {6} {7} {8}",
                            nodiscinserted ? "no disc" : "disc inserted",
                            poweredoff ? "powered off" : "powered on",
                            TrackNo,
                            playbackstatusstr,
                            toc_read_done ? "TOC clean" : "TOC dirty",
                            copy_protected ? "copy protected" : "not copy protected",
                            mono ? "mono audio" : "stereo audio",
                            digital_in_unlocked ? "digital input unlocked" : "digital input locked",
                            recsourcestr,
                            rec_possible ? "rec. allowed":"rec. disallowed"
                            );

                        AppendLog("MD: Repeat mode: {0}", repeatstr);



                        // request these since we now know the track number
                        Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: _currentrack);
                        Transmit_MDS_Message(MDS_TX_ReqTOCData);
                    }

                    // 7.12 DISC DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x21)
                    {
                        byte DiscData = ArrRep[6];

                        bool discerror = IsBitSet(DiscData, 3);
                        bool writeprotected = IsBitSet(DiscData, 2);
                        bool recordable = IsBitSet(DiscData, 0);

                        if (writeprotected)
                            label10.Text = toc_read_done ? "TOC Clean (WP)" : "TOC Dirty (WP)";
                        else
                            label10.Text = toc_read_done ? "TOC Clean" : "TOC Dirty";
                        label10.Font = toc_read_done ? new Font(DefaultFont, FontStyle.Regular) : new Font(DefaultFont, FontStyle.Bold);

                        AppendLog("MD: disc data: {0} {1} {2}",
                            discerror ?"disc error":"no error",
                            writeprotected ? "WP":"no wp",
                            recordable ? "pre-mastered":"recordable");
                    }

                    // 7.13 MODEL NAME
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x22)
                    {
                        string modelname = TrimNonAscii(DecodeAscii(ref ArrRep, 6));
                        AppendLog("MD: Model is {0}", modelname);
                        label9.Text = "Sony " + modelname;
                    }

                    // 7.14 REC DATA DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x24)
                    {
                        byte TrackNo = ArrRep[6];
                        byte Year = ArrRep[7];
                        byte Month = ArrRep[8];
                        byte Day = ArrRep[9];
                        byte Hour = ArrRep[10];
                        byte Min = ArrRep[11];
                        byte Sec = ArrRep[12];
                        AppendLog("MD: Track {0} was recorded at time XX{1:00}-{2:00}-{3:00}T{4:00}:{5:00}:{6:00}", TrackNo, Year, Month, Day, Hour, Min, Sec);
                    }

                    // 7.15 DISC NAME
                    if (ArrRep[4] == 0x20 && (ArrRep[5] == 0x48 || ArrRep[5] == 0x49))
                    {
                        byte Segment = ArrRep[6];
                        if (ArrRep[5] == 0x48)
                            discname = TrimNonAscii(DecodeAscii(ref ArrRep, 7));
                        else
                            discname += TrimNonAscii(DecodeAscii(ref ArrRep, 7));
                        label7.Text = String.Format("{0}", discname);
                        AppendLog("MD: Disc name part {1} is: {0}", TrimNonAscii(DecodeAscii(ref ArrRep, 7)), Segment);
                    }

                    // 7.16 TRACK NAME (1st packet)
                    if (ArrRep[4] == 0x20 && (ArrRep[5] == 0x4A || ArrRep[5] == 0x4B) && ArrRep.Length > 8)
                    {
                        byte Segment = ArrRep[6];
                        string currenttrackname = TrimNonAscii(DecodeAscii(ref ArrRep, 7));
                        if (_infocounter >= 0)
                        {
                            if (ArrRep[5] == 0x4A)
                            {
                                _infocounter++;
                                tracknames.Add(_infocounter, new StringBuilder(128));
                            }
                            StringBuilder sb;
                            tracknames.TryGetValue(_infocounter, out sb);
                            sb.Append(currenttrackname);
                            AppendLog("MD: Track {2} name part {1} is: {0}", currenttrackname, (ArrRep[5] == 0x4A) ? "1" : Segment.ToString(), _infocounter);
                        }
                        else
                            AppendLog("MD: Track name segment {1} is: {0}", currenttrackname, Segment);

                        if (_inforequest)
                        {
                            timer_Poll_GetInfo.Stop();
                            timer_Poll_GetInfo.Start();
                        }
                            

                        
                        
                    }

                    // 7.17 ALL NAME END
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x4C)
                    {
                        _infocounter = -1;
                        AppendLog("MD: ALL NAME END");

                        //UpdateDataGrid();
                    }

                    // 7.18 ELAPSED TIME
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x51)
                    {
                        byte TrackNo = ArrRep[6];
                        byte Min = ArrRep[8];
                        byte Sec = ArrRep[9];

                        ReceivedPlayingTrack(TrackNo);


                        TimeSpan ts_track;
                        tracklengths.TryGetValue(_currentrack, out ts_track);

                        TimeSpan ts_elapsed = new TimeSpan(0, Min, Sec);
                        TimeSpan remainingtime = ts_track - ts_elapsed;

                        AppendLog("MD: Track {0} elapsed time is {1:00}:{2:00}", TrackNo, Min, Sec);
                        label6.Text = String.Format("{0:00}:{1:00}/{2:00}:{3:00} (r: {4:00}:{5:00})", Min, Sec, 
                            (int)ts_track.TotalMinutes, ts_track.Seconds, 
                            (int)remainingtime.TotalMinutes, remainingtime.Seconds);
                    }

                    // 7.19 REC REMAIN
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x54)
                    {
                        byte Min = ArrRep[7];
                        byte Sec = ArrRep[8];
                        if (Min != _remainingrecordtime.Minutes || Sec != _remainingrecordtime.Seconds)
                            _remainingrecordtime = new TimeSpan(0, Min, Sec);
                        AppendLog("MD: Record remaining time is {0:00}:{1:00}", Min, Sec);
                    }

                    // 7.20 NAME REMAIN
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x55)
                    {
                        byte TrackNo = ArrRep[7];
                        byte RemainH = ArrRep[8];
                        byte RemainL  = ArrRep[9];
                        int remainingbytes = RemainH << 8 | RemainL;
                        AppendLog("MD: Track {0} maximum name size is {1}", TrackNo, remainingbytes);
                    }

                    // 7.21 TOC DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x60)
                    {
                        byte FirstTrackNo = ArrRep[7];
                        byte LastTrackNo = ArrRep[8];
                        _lasttrackno = LastTrackNo;

                        // call this since we are now sure what the last track is
                        ReceivedPlayingTrack(0, tracknounchanged: true);

                        byte Min = ArrRep[9];
                        byte Sec = ArrRep[10];
                        _disclength = new TimeSpan(0, Min, Sec);

                        AppendLog("MD: First track is {0}, last track is {1}. Recorded time is {2:00}:{3:00}", FirstTrackNo,LastTrackNo,Min,Sec);
                        label4.Text = String.Format("{0} tracks ({1}-{2}; {3:00}:{4:00}; {5:00}:{6:00} remaining)", 
                            1+LastTrackNo-FirstTrackNo, 
                            FirstTrackNo,LastTrackNo, 
                            Min, Sec,
                            (int)_remainingrecordtime.TotalMinutes, _remainingrecordtime.Seconds);

                        if (FirstTrackNo != 0 && LastTrackNo !=0)
                        {
                            try
                            {
                                numericUpDown1.Minimum = FirstTrackNo;
                                numericUpDown1.Maximum = LastTrackNo;
                                if (numericUpDown1.Value < FirstTrackNo)
                                    numericUpDown1.Value = FirstTrackNo;
                                if (numericUpDown1.Value > LastTrackNo && LastTrackNo != 0)
                                    numericUpDown1.Value = LastTrackNo;

                                numericUpDown1.Minimum = FirstTrackNo;
                                numericUpDown1.Maximum = LastTrackNo;
                            }
                            catch
                            {
                                // do nothing, it literally makes no difference if this errors out
                            }
                        }
                        else
                        {
                            numericUpDown1.Minimum = 1;
                            numericUpDown1.Maximum = 255;
                            numericUpDown1.Value = 1;
                        }

                    }

                    // 7.22 TRACK TIME DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x62 && ArrRep.Length > 8)
                    {
                        if (ArrRep[6] == 0x01 && ArrRep[7] == _currentrack)
                        {
                            byte Min = ArrRep[8];
                            byte Sec = ArrRep[9];

                            TimeSpan newts = new TimeSpan(0, Min, Sec);

                            TimeSpan ts;
                            // update index if already present
                            if (tracklengths.TryGetValue(_currentrack, out ts))
                                tracklengths[_currentrack] = newts;
                            else
                                tracklengths.Add(_currentrack, newts);

                            UpdateDataGridBold(_currentrack);

                            //_currtracklen_min = Min;
                            //_currtracklen_sec = Sec;
                            AppendLog("MD: Current track length is {0:00}:{1:00}", Min, Sec);
                            if (!checkBox2_Elapsed.Checked)
                                label6.Text = GetTrackLenFormatted(_currentrack);
                        }
                    }

                    // 7.23 DISC EXIST
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x82)
                    {
                        AppendLog("MD: TOC read, disc ready");
                    }

                    // 7.24 1 TRACK END
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x83)
                    {
                        AppendLog("MD: Track changed");
                        Transmit_MDS_Message(MDS_TX_ReqStatus);
                        StringBuilder sb;
                        tracknames.TryGetValue(_currentrack, out sb);
                        if (sb != null)
                            label8.Text = sb.ToString();
                    }

                    // 7.25 NO DISC NAME
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x85)
                    {
                        AppendLog("MD: Current disc not named");
                    }

                    // 7.26 NO TRACK NAME
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x86)
                    {
                        AppendLog("MD: Current track not named");
                    }

                    // 7.27 WRITE PACKET RECEIVED
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x87)
                    {
                        AppendLog("MD: Write packet acknowledge");
                    }

                    // 7.28 NO TOC DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x89)
                    {
                        AppendLog("MD: No TOC data present or no disc inserted");
                    }

                    /* skipped some editing commands */

                    // 7.33 UNDFINED COMMAND
                    if (ArrRep[4] == 0x40 && ArrRep[5] == 0x01)
                    {
                        AppendLog("MD: Unknown command received by MD");
                    }

                    // 7.34 IMPOSSIBLE
                    if (ArrRep[4] == 0x40 && ArrRep[5] == 0x03)
                    {
                        AppendLog("MD: Reports previous command not possible");
                    }
                }

            }
        }

        // this function handles GUI updates when we receive track playing info
        // tracknounchanged flags that we received e.g. a last track number
        // note that track 0 is the stopped/new disc/no disc state
        private void ReceivedPlayingTrack(byte TrackNo, bool tracknounchanged = false)
        {
            if (!tracknounchanged)
                _currentrack = TrackNo;

            if (_currentrack > 0)
                label2.Text = String.Format("Track {0}/{1}", _currentrack, _lasttrackno);
            else
                label2.Text = String.Format("Track -/{0}", _lasttrackno);

            // update the datagridview active track indicator
            UpdateDataGridBold(_currentrack);

            // try to update the track title label
            StringBuilder sb;
            if (tracknames.TryGetValue(_currentrack, out sb))
                label8.Text = sb.ToString();
            else
                label8.Text = "Track Title";
        }

        private string GetTrackLenFormatted(int key)
        {
            if (key == 0)
                return String.Format("{0:00}:{1:00}", (int)_disclength.TotalMinutes, (int)_disclength.Seconds);

            TimeSpan ts;
            if (tracklengths.TryGetValue(key, out ts))
                return String.Format("{0:00}:{1:00}", (int)ts.TotalMinutes, (int)ts.Seconds);
            else
                return "";
        }

        // convert our populated track-name index when a full disc title sequence has been received
        private void UpdateDataGrid()
        {
            dataGridView1.Rows.Clear();

            // first index is disc name, this also makes the track and array indices line up
            dataGridView1.Rows.Add("Disc", discname, discname.Length == 0 ? true : false, GetTrackLenFormatted(0));

            foreach (DataGridViewCell cell in dataGridView1.Rows[0].Cells)
                cell.Style.Font = new Font(DefaultFont, FontStyle.Bold);

            foreach (var track in tracknames)
            {

                dataGridView1.Rows.Add(track.Key, track.Value.ToString(), track.Value.ToString().Length == 0 ? true : false, GetTrackLenFormatted(track.Key));
            }

            UpdateDataGridBold(_currentrack);

        }

        // if we know the current track, bold it in the datagrid
        private void UpdateDataGridBold(int trackplaying)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (dataGridView1.Rows.IndexOf(row) == 0)
                {
                    row.Cells[3].Value = GetTrackLenFormatted(0);
                    continue;
                }

                if (row.Cells.Count == 0)
                    continue;

                row.Cells[3].Value = GetTrackLenFormatted((int)row.Cells[0].Value);

                if (row.Cells[0].Value != null && (int)row.Cells[0].Value == trackplaying)
                {
                    row.Cells[0].Style.Font = new Font(DefaultFont, FontStyle.Bold);
                    row.Cells[0].Style.BackColor = Color.Black;
                    row.Cells[0].Style.ForeColor = Color.White;
                }
                else
                {
                    row.Cells[0].Style.Font = new Font(DefaultFont, FontStyle.Regular);
                    row.Cells[0].Style.BackColor = Color.White;
                    row.Cells[0].Style.ForeColor = Color.Black;
                }
                    
            }

        }

        private void richTextBox_Log_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            richTextBox_Log.SelectionStart = richTextBox_Log.Text.Length;
            // scroll it automatically
            richTextBox_Log.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_SetRemoteOn, priority: true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_SetRemoteOff);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PlayPause, allowduplicates: true, priority: true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqModelName);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Play, priority: true);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Stop, priority: true);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PrevTrack, priority: true);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_NextTrack, priority: true);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Eject);
        }

        bool _inforequest = false;

        private void button10_Click(object sender, EventArgs e)
        {
            
            checkBox1_Autopoll.Checked = false;
            checkBox2_Elapsed.Checked = false;
            // reset list of names
            _infocounter = 0;
            tracknames.Clear();
            Transmit_MDS_Message(MDS_TX_SetRemoteOn, delay: 500);
            Transmit_MDS_Message(MDS_TX_DisableElapsedTimeTransmit);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqModelName);
            Transmit_MDS_Message(MDS_TX_ReqRemainingRecordTime);
            //Transmit_MDS_Message(MDS_TX_ReqDiscName, delay: 500);
            Transmit_MDS_Message(MDS_TX_ReqDiscAndTrackNames, delay:3000);
            _inforequest = true;
            checkBox1_Autopoll.Checked = true;
            checkBox2_Elapsed.Checked = true;
        }

        private void DoUpdateTask()
        {
            Transmit_MDS_Message(MDS_TX_ReqTOCData);
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqDiscData);
            Transmit_MDS_Message(MDS_TX_ReqTrackRemainingNameSize, tracknumber: _currentrack);
            if (checkBox2_Elapsed.Checked)
                Transmit_MDS_Message(MDS_TX_EnableElapsedTimeTransmit);
            else
                Transmit_MDS_Message(MDS_TX_DisableElapsedTimeTransmit);

            if (checkBox3.Checked)
                Transmit_MDS_Message(MDS_TX_AutoPauseOn);
            else
                Transmit_MDS_Message(MDS_TX_AutoPauseOff);

            //Transmit_MDS_Message(MDS_TX_ReqTrackTime, _currentrack);
            //Transmit_MDS_Message(MDS_TX_ReqTOCData);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            DoUpdateTask();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqTrackName, (byte)numericUpDown1.Value);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: _currentrack);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: _currentrack);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqTOCData);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_StartPlayAtTrack, tracknumber: (byte)numericUpDown1.Value);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PausePlayAtTrack, tracknumber: (byte)numericUpDown1.Value);
        }



        private void button15_Click(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == 0)
                return;
            AppendLog("Playing track {0}", e.RowIndex);
            Transmit_MDS_Message(MDS_TX_StartPlayAtTrack, tracknumber: (byte)(e.RowIndex));
        }

        private void label7_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqDiscName);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1_Autopoll.Checked)
            {
                timer1_Maintenance.Interval = 5000;
                DoUpdateTask();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            DoUpdateTask();
        }

        // this timer is continuously reset whenever a track title message is received
        // and the _inforequest flag is set
        // so it expires once those messages stop rolling in
        // the serial transmit routine detects that this timer is active and the flag is set
        // and inhibits any further transmissions
        private void timer_Poll_GetInfo_Tick(object sender, EventArgs e)
        {
            _inforequest = false;
            UpdateDataGrid();
            timer_Poll_GetInfo.Enabled = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            DoUpdateTask();
        }

        private void SonyMDRemote_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
                button5_Prev.PerformClick();
            if (e.Button == MouseButtons.XButton2)
                button6_Next.PerformClick();
        }

        private void button16_Click(object sender, EventArgs e)
        {

            checkBox1_Autopoll.Checked = false;
            checkBox2_Elapsed.Checked = false;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells.Count == 0)
                    continue;
                //if (dataGridView1.Rows.IndexOf(row) == dataGridView1.Rows.Count - 1)
                //    continue;
                DataGridViewCheckBoxCell checkcell = row.Cells[2] as DataGridViewCheckBoxCell;
                DataGridViewTextBoxCell textcell = row.Cells[1] as DataGridViewTextBoxCell;
                if (dataGridView1.Rows.IndexOf(row) == 0 && (bool)checkcell.Value == true)
                    Transmit_MDS_Write(textcell.Value.ToString());
                else if ((dataGridView1.Rows.IndexOf(row) != 0) && (bool)checkcell.Value == true)
                    Transmit_MDS_Write(textcell.Value.ToString(), (byte)dataGridView1.Rows.IndexOf(row));

            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_RowValidated(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView gridview = (DataGridView)sender;
            DataGridViewRow row = gridview.Rows[e.RowIndex];
            if (row.Cells.Count == 0)
                return;
            DataGridViewCheckBoxCell checkcell = row.Cells[2] as DataGridViewCheckBoxCell;
            checkcell.Value = true;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            richTextBox_Log.Clear();
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex <= numericUpDown1.Maximum && e.RowIndex >= numericUpDown1.Minimum)
                numericUpDown1.Value = e.RowIndex;
        }
    }
}
