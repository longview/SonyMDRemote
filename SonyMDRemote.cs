using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
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

            // add the mouse-down handler to all element to allow the nav-keys to work anywhere
            AllSubControls(this).OfType<Control>().ToList()
                .ForEach(o => o.MouseDown += SonyMDRemote_MouseDown);


            string builddate = "";
            try
            {
                builddate =  Properties.Resources.BuildDate;
                DateTime builddate_parsed = DateTime.Parse(builddate);
                builddate = builddate_parsed.ToString("s");
            }
            catch
            {
                builddate = "Unknown build time";
            }


            this.Text = String.Format("LA2YUA SonyMDRemote {0} {1}", VersionString, ReleaseString);
#if LOGGING
            this.Text = String.Format("LA2YUA SonyMDRemote {0} {1}/{2}", VersionString, ReleaseString, builddate);
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

            AppendLog("Program start version {0} {1} built {2}", VersionString, ReleaseString, builddate);
            AppendLog("See https://github.com/longview/SonyMDRemote for the latest release.");
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

            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();

            SendMessage(progressBar1.Handle,
              0x400 + 16, //WM_USER + PBM_SETSTATE
              0x0003, //PBST_PAUSED
              0);

#if !LOGGING
            button17.Visible = false;
#endif
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern uint SendMessage(IntPtr hWnd,
  uint Msg,
  uint wParam,
  uint lParam);

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

        MDSContext mdctx = new MDSContext();

        string VersionString = "v0.5a-dev";

#if LOGGING
        string ReleaseString = "debug";
#elif !LOGGING
        string ReleaseString = "release";
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

        // this function handles GUI updates when we receive track playing info
        // tracknounchanged flags that we received e.g. a last track number
        // note that track 0 is the stopped/new disc/no disc state
        private void ReceivedPlayingTrack()
        {
            if (mdctx.CurrentTrack > 0)
                label2.Text = String.Format("Track {0}/{1}", mdctx.CurrentTrack, mdctx.Disc.LastTrack);
            else
                label2.Text = String.Format("Track -/{0}", mdctx.Disc.LastTrack);

            if (mdctx.CurrentTrack == 0 && mdctx.Disc.HasRecordedDate())
                label12_timestamp.Text = "Disc Recorded: " + mdctx.Disc.RecordedDate.ToString();
            else if (mdctx.CurrentTrack > 0 && mdctx.GetCurrentTrackOrEmpty().HasRecordedDate())
                label12_timestamp.Text = "Recorded: " + mdctx.GetCurrentTrackOrNull().RecordedDate.ToString();
            else
                label12_timestamp.Text = "Recorded: N/A";

            progressBar1.Value = (int)Math.Min(mdctx.CurrentTrackProgress * 1000, 999) + 1;
            if (checkBox2_Elapsed.Checked)
            {
                MDTrackData track = mdctx.GetCurrentTrackOrEmpty();
                label6.Text = String.Format("{0:00}:{1:00}/{2:00}:{3:00} (r: {4:00}:{5:00})",
                    (int)mdctx.CurrentTrackElapsedTime.TotalMinutes, mdctx.CurrentTrackElapsedTime.Seconds,
                                        (int)track.Length.TotalMinutes, track.Length.Seconds,
                                        (int)mdctx.CurrentTrackRemainingTime.TotalMinutes, mdctx.CurrentTrackRemainingTime.Seconds);
            }

            // update the datagridview active track indicator
            UpdateDataGridBold();
            UpdateTrackTitle();
        }

        private void UpdateTrackTitle()
        {
            label8.Text = mdctx.GetCurrentTrackTitleOrDefault();
        }

        private string GetTrackLenFormatted(int key)
        {
            if (key == 0)
                return String.Format("{0:00}:{1:00}", (int)mdctx.Disc.Length.TotalMinutes, (int)mdctx.Disc.Length.Seconds);

            if (mdctx.Disc.Tracks.ContainsKey(key) && mdctx.Disc.Tracks[key].HasLength())
                return String.Format("{0:00}:{1:00}", (int)mdctx.Disc.Tracks[key].Length.TotalMinutes, (int)mdctx.Disc.Tracks[key].Length.Seconds);
            else
                return "";
        }

        // convert our populated track-name index when a full disc title sequence has been received
        private void UpdateDataGrid()
        {
            dataGridView1.SuspendLayout();
            if (dataGridView1.Rows.Count > mdctx.Disc.Tracks.Count + 1)
                dataGridView1.Rows.Clear();

            // first index is disc name, this also makes the track and array indices line up
            if (dataGridView1.Rows.Count == 0)
                dataGridView1.Rows.Add("Disc", mdctx.Disc.Title, mdctx.Disc.Title.Length == 0 ? true : false, GetTrackLenFormatted(0));
            else
            {
                dataGridView1.Rows[0].Cells[0].Value = "Disc";
                dataGridView1.Rows[0].Cells[1].Value = mdctx.Disc.Title;
                dataGridView1.Rows[0].Cells[2].Value = mdctx.Disc.Title.Length == 0 ? true : false;
                dataGridView1.Rows[0].Cells[3].Value = GetTrackLenFormatted(0);
            }

            foreach (DataGridViewCell cell in dataGridView1.Rows[0].Cells)
                cell.Style.Font = new Font(DefaultFont, FontStyle.Bold);

            foreach (var track in mdctx.Disc.Tracks)
            {
                // if there is already a row like this
                if (dataGridView1.Rows.Count - 1 >= track.Key)
                {
                    dataGridView1.Rows[track.Key].Cells[0].Value = track.Key;
                    dataGridView1.Rows[track.Key].Cells[1].Value = track.Value.Title.ToString();
                    dataGridView1.Rows[track.Key].Cells[2].Value = track.Value.Title.ToString().Length == 0 ? true : false;
                    dataGridView1.Rows[track.Key].Cells[3].Value = GetTrackLenFormatted(track.Key);
                }
                else
                    dataGridView1.Rows.Add(track.Key, track.Value.Title.ToString(), track.Value.Title.ToString().Length == 0 ? true : false, GetTrackLenFormatted(track.Key));
            }

            //dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0];

            dataGridView1.ResumeLayout();

            UpdateDataGridBold();

        }

        // if we know the current track, bold it in the datagrid
        private void UpdateDataGridBold(bool setfocus = false)
        {
            Font bold = new Font(DefaultFont, FontStyle.Bold);
            Font regular = new Font(DefaultFont, FontStyle.Regular);

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

                if (row.Cells[0].Value != null && (int)row.Cells[0].Value == mdctx.CurrentTrack)
                {
                    
                    for (int i = 0; i < 4; i++)
                    {
                        row.Cells[i].Style.Font = bold;
                        row.Cells[i].Style.BackColor = Color.Black;
                        row.Cells[i].Style.ForeColor = Color.White;
                    }

                    if (setfocus)
                        dataGridView1.CurrentCell = row.Cells[0];
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        row.Cells[i].Style.Font = regular;
                        row.Cells[i].Style.BackColor = Color.White;
                        row.Cells[i].Style.ForeColor = Color.Black;
                    }
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
            checkBox1_Autopoll.Checked = false;
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
            Transmit_MDS_Message(MDS_TX_Eject, priority: true);
        }

        bool _inforequest = false;

        private void button10_Click(object sender, EventArgs e)
        {
            
            checkBox1_Autopoll.Checked = false;
            checkBox2_Elapsed.Checked = false;
            // reset list of names
            //_infocounter = 0;
            // TODO: need to make a trackname-clear function
            //tracknames.Clear();
            

            // stop any commands in queue
            timer_Poll_Time.Stop();

            // clear all the previous non priority commands
            commandqueue.Clear();

            // dump all these in the priority queue to get them out before the track change
            // these will be executed before any non priority commands regardless of state

            //Transmit_MDS_Message(MDS_TX_SetPowerOn, batch: true, priorityqueue: true, delay: 100); // should only be issued for E11/E52 models
            Transmit_MDS_Message(MDS_TX_SetRemoteOn, batch: true, priorityqueue: true, delay: 100);
            Transmit_MDS_Message(MDS_TX_DisableElapsedTimeTransmit, batch: true, priorityqueue: true, delay: 100);
            Transmit_MDS_Message(MDS_TX_ReqStatus, batch: true, priorityqueue: true, delay: 100);
            Transmit_MDS_Message(MDS_TX_ReqTOCData, batch: true, priorityqueue: true, delay: 100);
            Transmit_MDS_Message(MDS_TX_ReqModelName, batch: true, priorityqueue: true, delay: 100);
            Transmit_MDS_Message(MDS_TX_ReqRemainingRecordTime, batch: true, priorityqueue: true, delay: 100);
            // re-request status to trigger GUI update again
            Transmit_MDS_Message(MDS_TX_ReqStatus, batch: true, priorityqueue: true, allowduplicates: true);

            // this triggers the whole sequence
            Transmit_MDS_Message(MDS_TX_ReqDiscAndTrackNames, delay:1000);
            _inforequest = true;
            checkBox1_Autopoll.Checked = true;
            checkBox2_Elapsed.Checked = true;
        }

        private void DoUpdateTask()
        {
            if (mdctx.PlayerState != MDSContext.MDSStatusD1.EJECT)
            {
                // some of these might not be required since status receipt triggers other reads
                // but doesn't seem to cause any problems
                Transmit_MDS_Message(MDS_TX_ReqTOCData);
                Transmit_MDS_Message(MDS_TX_ReqDiscData);
                // some commands are only supported or sensible in specific modes

                if (mdctx.CurrentTrack > 0)
                    Transmit_MDS_Message(MDS_TX_ReqTrackRemainingNameSize, tracknumber: mdctx.CurrentTrack);
                Transmit_MDS_Message(MDS_TX_ReqTrackRecordDate, tracknumber: mdctx.CurrentTrack);
            }
                
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            
            

            

            if (mdctx.PlayerState == MDSContext.MDSStatusD1.PAUSE || mdctx.PlayerState == MDSContext.MDSStatusD1.PLAY)
            {
                if (checkBox2_Elapsed.Checked)
                    Transmit_MDS_Message(MDS_TX_EnableElapsedTimeTransmit);
                else
                    Transmit_MDS_Message(MDS_TX_DisableElapsedTimeTransmit);
            }

            
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
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: mdctx.CurrentTrack);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqStatus);
            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: mdctx.CurrentTrack);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_ReqTOCData);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_StartPlayAtTrack, tracknumber: (byte)numericUpDown1.Value, priority: true);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_PausePlayAtTrack, tracknumber: (byte)numericUpDown1.Value, priority: true);
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

            DataGridView gridview = (DataGridView)sender;
            DataGridViewRow row = gridview.Rows[e.RowIndex];
            if (row.Cells.Count == 0)
                return;

            int key = int.Parse(row.Cells[0].Value.ToString());
            AppendLog("Playing track {0}", key);
            Transmit_MDS_Message(MDS_TX_StartPlayAtTrack, tracknumber: (byte)(key), priority: true);
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
            /*
            if (checkBox2_Elapsed.Checked)
                progressBar1.Visible = true;
            else
                progressBar1.Visible = false;*/
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
            // set focus to currently playing track
            UpdateDataGridBold(setfocus: false);
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
            if (e.RowIndex == 0)
                return;

            DataGridView gridview = (DataGridView)sender;
            DataGridViewRow row = gridview.Rows[e.RowIndex];
            if (row.Cells.Count == 0)
                return;

            int key = int.Parse(row.Cells[0].Value.ToString());

            if (key <= numericUpDown1.Maximum && key >= numericUpDown1.Minimum)
                numericUpDown1.Value = key;
        }

        private void label12_timestamp_Click(object sender, EventArgs e)
        {

        }

        private void label7_disctitle_TextChanged(object sender, EventArgs e)
        {
            scaleFont((Label)sender);
        }

        private void label8_TextChanged(object sender, EventArgs e)
        {
            scaleFont((Label)sender);
        }

        private void label8_Click(object sender, EventArgs e)
        {
            scaleFont((Label)sender);
        }

        private void linkLabel_savetracks_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            string outputfilename = String.Format("SonyMDTracklist_{1}_{0}.txt", DateTime.UtcNow.ToString("o").Replace(':', '_'), mdctx.Disc.Title);

            saveFileDialog1.FileName = outputfilename;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                mdctx.Disc.Serialize(new StreamWriter(saveFileDialog1.FileName), VersionString, ReleaseString);

            AppendLog("Saved track data to file {0}", saveFileDialog1.FileName);
        }

        private void linkLabel_loadtracks_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            var openfile = openFileDialog1.ShowDialog();
            if (openfile == DialogResult.OK)
                mdctx.Disc.Import(new StreamReader(openFileDialog1.FileName));
            AppendLog("Loaded track data from file {0}", openFileDialog1.FileName);
            AppendLog("Header was: {0}", mdctx.Disc.ImportHeader);
            UpdateDataGrid();
            ReceivedPlayingTrack();
            ReceivedTOCData();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_SetPowerOff);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Transmit_MDS_Message(MDS_TX_SetPowerOn);
        }

        private void timer_TXBusy_Tick(object sender, EventArgs e)
        {
            label_TXIndicator.Text = "-";
        }

        private void label_RXIndicator_Click(object sender, EventArgs e)
        {
            label_RXIndicator.Text = "-";
        }

        private void timer_RXBusy_Tick(object sender, EventArgs e)
        {
            label_RXIndicator.Text = "-";
        }
    }
}
