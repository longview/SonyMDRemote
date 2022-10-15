using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonyMDRemote
{
    public partial class SonyMDRemote : Form
    {
        public SonyMDRemote()
        {
            InitializeComponent();

            label1.Text = String.Format("LA2YUA SonyMDRemote {0}", VersionString);

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
            AppendLog("Program start version {0}", VersionString);
            AppendCmdLog("Program start version {0}", VersionString);
            Update_COM_List(true);
        }

        string VersionString = "v0.1a";

        StreamWriter logfile;
        StreamWriter logfile_cmd;

        private void AppendLog(string s, params object[] format)
        {
            if (logfile == null)
            {
                return;
            }
            string datestamp = String.Format("{0}Z: ", DateTime.UtcNow.ToString("s"));
            string datestamp_log = String.Format("{0}Z: ", DateTime.UtcNow.ToString("o"));
            string data = String.Format("{0}\r\n", String.Format(s, format));
            richTextBox_Log.AppendText(datestamp);
            richTextBox_Log.AppendText(data);
            logfile.Write(datestamp_log + data);

            AppendCmdLog(data);
        }

        private void AppendCmdLog(string s, params object[] format)
        {
            if (logfile_cmd == null)
            {
                return;
            }
            string datestamp = String.Format("{0}Z: ", DateTime.UtcNow.ToString("o"));
            string data = String.Format("{0}\r\n", String.Format(s, format));
            logfile_cmd.Write(datestamp + data);
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
            timer1_Maintenance.Interval = 10000;
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

            logfile.Flush();

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
        byte[] MDS_TX_WriteDiscName1 = new byte[] { 0x20, 0x70, 0x01 }; // next 16 bytes is name data
        byte[] MDS_TX_WriteDiscName2 = new byte[] { 0x20, 0x71}; // next byte is packet number, next 16 bytes is name data, null terminated

        // 6.43 TRACK NO. NAME WRITE
        // Playback must be stopped first
        // supports ASCII values 0, 0x20-0x5A, 0x5E-0x7A
        // should wait for WRITE PACKET RECEIVED message between writes
        // final namedata byte must be 0 to effect write
        // then finally WRITE COMPLETE
        byte[] MDS_TX_WriteTrackName1 = new byte[] { 0x20, 0x72, }; // next byte is track number, next 16 bytes is name data, null terminated
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

        // track management
        byte[] MDS_TX_StartPlayAtTrack = new byte[] { 0x03, 0x42, 0x01 }; // next byte is track number
        byte[] MDS_TX_PausePlayAtTrack = new byte[] { 0x03, 0x43, 0x01 }; // next byte is track number, pauses at the start of specific track

        byte[] MDS_TX_EnableElapsedTimeTransmit = new byte[] { 0x07, 0x10 };
        byte[] MDS_TX_DisableElapsedTimeTransmit = new byte[] { 0x07, 0x11 };

        private void Transmit_MDS_Message(byte[] data, byte tracknumber = 0)
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

            serialPort1.Write(txdata.ToArray(), 0, txdata.Count);
            //Start_Timeout();
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
        private int serialRXStateCounter = 0;
        private List<byte> serialRXData = new List<byte>();
        delegate void SetSerialDataInputCallback(byte[] text);

        byte _currentrack;

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
            StringBuilder sb = new StringBuilder();


            foreach (byte currentchar in rxdata)
            {
                switch ((char)currentchar)
                {
                    case (char)0x6F:
                        receiverstate = serialRXState.serialRXState_Started;
                        serialRXData.Clear();
                        //AppendLog("Header received");
                        break;
                    case (char)0xFF:
                        if (serialRXData.Count+1 != serialRXData[1])
                        {
                            AppendLog("Found terminator but count wrong: {1} but terminator at {0}", serialRXData.Count, serialRXData[1]);
                        }
                        serialRXData.Add(currentchar);
                        receiverstate = serialRXState.serialRxState_Stop;


                        //timer_Serial_Timeout.Stop();
                        byte[] ArrRep = serialRXData.ToArray();

                        AppendLog("MD sent: {0}, ASCII: {1}", BitConverter.ToString(ArrRep), TrimNonAscii(System.Text.Encoding.ASCII.GetString(ArrRep)));

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

                            label2.Text = String.Format("Track {0}", TrackNo);
                            _currentrack = TrackNo;

                            // these two seems to be inverted or useless on the E12
                            bool discinserted = IsBitSet(Data1, 5);
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

                            bool toc_read_done = IsBitSet(Data2, 7);
                            bool rec_possible = IsBitSet(Data2, 5);

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

                            AppendLog("MD: Status dump {0} {1} track no. {2} {3} {4} {5} {6} {7} {8}", 
                                discinserted ? "disc inserted":"no disc", 
                                poweredoff ? "powered on":"powered off",
                                TrackNo,
                                playbackstatusstr,
                                toc_read_done ? "TOC read":"TOC not read yet",
                                copy_protected ? "copy protected":"not copy protected",
                                mono ? "mono audio":"stereo audio",
                                digital_in_unlocked ? "digital input unlocked":"digital input locked",
                                recsourcestr
                                );
                        }

                        // 7.12 DISC DATA
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x21)
                        {
                            byte DiscData = ArrRep[6];

                            bool discerror = IsBitSet(DiscData, 3);
                            bool writeprotected = IsBitSet(DiscData, 2);
                            bool recordable = IsBitSet(DiscData, 0);

                            AppendLog("MD: disc data: {0} {1} {2}",
                                discerror ?"disc error":"no error",
                                writeprotected ? "write protected":"recordable");
                        }

                        // 7.13 MODEL NAME
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x22)
                        {
                            string modelname = TrimNonAscii(DecodeAscii(ref ArrRep, 6));
                            AppendLog("MD: Model is {0}", modelname);
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
                            AppendLog("MD: Disc name part {1} is: {0}", TrimNonAscii(DecodeAscii(ref ArrRep, 7)), Segment);
                        }

                        // 7.16 TRACK NAME (1st packet)
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x4A && ArrRep.Length > 8)
                        {
                            byte Segment = ArrRep[6];
                            AppendLog("MD: Track {1} name part 1 is: {0}", TrimNonAscii(DecodeAscii(ref ArrRep, 7)), Segment);
                        }

                        // 7.16 TRACK NAME (2nd packet)
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x4B && ArrRep.Length > 8)
                        {
                            byte Segment = ArrRep[6];
                            AppendLog("MD: Track name part {1} is: {0}", TrimNonAscii(DecodeAscii(ref ArrRep, 7)), Segment);
                        }

                        // 7.17 ALL NAME END
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x4C)
                        {
                            AppendLog("MD: ALL NAME END");
                        }

                        // 7.18 ELAPSED TIME
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x51)
                        {
                            byte TrackNo = ArrRep[6];
                            byte Min = ArrRep[8];
                            byte Sec = ArrRep[9];
                            AppendLog("MD: Track {0} elapsed time is {1:00}:{2:00}", TrackNo, Min, Sec);
                        }

                        // 7.19 REC REMAIN
                        if (ArrRep[4] == 0x20 && ArrRep[5] == 0x54)
                        {
                            byte Min = ArrRep[7];
                            byte Sec = ArrRep[8];
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
                            byte Min = ArrRep[9];
                            byte Sec = ArrRep[10];
                            AppendLog("MD: First track is {0}, last track is {1}. Recorded time is {2:00}:{3:00}", FirstTrackNo,LastTrackNo,Min,Sec);
                            label4.Text = String.Format("{0} tracks ({1}-{2}; {3:00}:{4:00})", 1+LastTrackNo-FirstTrackNo, FirstTrackNo,LastTrackNo, Min, Sec);

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
                                    // do nothing, it literally makes no difference
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
                                AppendLog("MD: Current track length is {0:00}:{1:00}", Min, Sec);
                                label6.Text = String.Format("{0:00}:{1:00}", Min, Sec);
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


                        //try
                        //{
                        //    UInt32 Current_DAC_Value = BitConverter.ToUInt32(decodedbytes, 0);
                        //}
                        //catch (Exception e)
                        //{
                        //    AppendLog("Decode error! {0}", e.Message);
                        //    return;
                        //}

                        //if (rx_timeoutstate == serialtimeoutstate.Timeout)
                        //{
                        //    AppendLog("Now receiving valid data after previous timeout");
                        //}
                        //else if (rx_timeoutstate == serialtimeoutstate.Idle)
                        //{
                        //    AppendLog("Valid data received from device");
                        //}
                        //rx_timeoutstate = serialtimeoutstate.Received;

                        //UpdateGUI(ref lastrxstatus);
                        break;
                }

                serialRXData.Add(currentchar);
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
            Transmit_MDS_Message(MDS_TX_SetRemoteOn);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_SetRemoteOff);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PlayPause);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqModelName);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Play);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Stop);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PrevTrack);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_NextTrack);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_Eject);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqDiscAndTrackNames);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqDiscName);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqTrackName, _currentrack);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, _currentrack);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, _currentrack);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqTOCData);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_StartPlayAtTrack, (byte)numericUpDown1.Value);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PausePlayAtTrack, (byte)numericUpDown1.Value);
        }

        private void timer_Poll_Time_Tick(object sender, EventArgs e)
        {

        }
    }
}
