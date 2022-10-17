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
    public partial class SonyMDRemote
    {
        // this filename is kind of a lie
        // it's just a place to stash the receiver functions out of the way :)


        enum serialRXState
        {
            serialRXState_Idle,
            serialRXState_Started,
            serialRXState_Escape,
            serialRxState_Stop
        };

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
        byte _firsttrackno = 0;
        TimeSpan _remainingrecordtime = new TimeSpan(0);
        TimeSpan _disclength = new TimeSpan(0);
        string discname;
        MDS_Status_D1 playbackstatus = MDS_Status_D1.reserved;
        MDS_Status_D1 lastplaybackstatus = MDS_Status_D1.reserved;

        string laststatusstr = String.Empty;
        string lastrptstr = String.Empty;
        string lastdiscstatusstr = String.Empty;
        string lastrecdatestr = String.Empty;

        // list of track names
        IDictionary<int, StringBuilder> tracknames = new Dictionary<int, StringBuilder>();

        // list of track lengths
        IDictionary<int, TimeSpan> tracklengths = new Dictionary<int, TimeSpan>();

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

                        playbackstatus = MDS_Status_D1.reserved;
                        string playbackstatusstr = "Unknown";
                        switch (Data1 & 0x0f)
                        {
                            case 0x0:
                                playbackstatus = MDS_Status_D1.STOP;
                                playbackstatusstr = "Stop";
                                label3_playstatusindicator.ForeColor = Color.Black;
                                break;
                            case 0x1:
                                playbackstatus = MDS_Status_D1.PLAY;
                                playbackstatusstr = "Play";
                                label3_playstatusindicator.ForeColor = Color.Green;
                                break;
                            case 0x2:
                                playbackstatus = MDS_Status_D1.PAUSE;
                                playbackstatusstr = "Pause";
                                label3_playstatusindicator.ForeColor = Color.Orange;
                                break;
                            case 0x3:
                                playbackstatus = MDS_Status_D1.EJECT;
                                playbackstatusstr = "Eject";
                                label3_playstatusindicator.ForeColor = Color.Black;
                                break;
                            case 0x4:
                                playbackstatus = MDS_Status_D1.REC_PLAY;
                                playbackstatusstr = "Rec. Play";
                                label3_playstatusindicator.ForeColor = Color.Red;
                                break;
                            case 0x5:
                                playbackstatus = MDS_Status_D1.REC_PAUSE;
                                label3_playstatusindicator.ForeColor = Color.OrangeRed;
                                playbackstatusstr = "Rec. Pause";
                                break;
                            case 0x6:
                                playbackstatus = MDS_Status_D1.rehearsal;
                                playbackstatusstr = "rehearsal";
                                break;

                        }

                        label3_playstatusindicator.Text = playbackstatusstr;

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



                        string statusstr = String.Format("MD: Status: {0}, {1}, {9}, track no. {2}, {3}, {4}, {5}, {6}, {7}, {8}.",
                            nodiscinserted ? "No disc" : "Disc",
                            poweredoff ? "Off" : "On",
                            TrackNo,
                            playbackstatusstr,
                            toc_read_done ? "TOC clean" : "TOC dirty",
                            copy_protected ? "WP" : "no WP",
                            mono ? "Mono" : "Stereo",
                            digital_in_unlocked ? "DIN unlock" : "DIN lock",
                            recsourcestr,
                            rec_possible ? "Rec. allowed" : "Rec. disallowed"
                            );

                        if (playbackstatus == MDS_Status_D1.EJECT)
                            label10.Text = "No Disc";
                        else
                            label10.Text = nodiscinserted ? "No Disc" : toc_read_done ? "TOC Clean" : "TOC Dirty";
                        label10.Font = toc_read_done ? new Font(DefaultFont, FontStyle.Regular) : new Font(DefaultFont, FontStyle.Bold);
#if LOGGING
                        AppendLog(statusstr);
#else
                        if (!laststatusstr.Equals(statusstr))
                            AppendLog(statusstr);
#endif
                        laststatusstr = statusstr;



#if LOGGING
                        AppendLog("MD: Repeat mode: {0}", repeatstr);
#else
                        if (!lastrptstr.Equals(repeatstr))
                            AppendLog("MD: Repeat mode: {0}", repeatstr);
#endif
                        lastrptstr = repeatstr;


                        if (playbackstatus != MDS_Status_D1.EJECT)
                        {
                            // request these since we now know the track number
                            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: _currentrack);
                            Transmit_MDS_Message(MDS_TX_ReqTOCData);
                        }

                        if (playbackstatus == MDS_Status_D1.EJECT)
                        {
                            label12_timestamp.Text = "Recorded: N/A";
                            discname = "No Disc";
                            label7_disctitle.Text = "No Disc";
                            _currentrack = 0;
                            if (lastplaybackstatus != MDS_Status_D1.EJECT)
                                tracklengths.Clear();
                            lastplaybackstatus = playbackstatus;
                        }
                    }

                    // 7.12 DISC DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x21)
                    {
                        byte DiscData = ArrRep[6];

                        bool discerror = IsBitSet(DiscData, 3);
                        bool writeprotected = IsBitSet(DiscData, 2);
                        bool recordable = IsBitSet(DiscData, 0);

                        /*if (writeprotected)
                            label10.Text = toc_read_done ? "TOC Clean (WP)" : "TOC Dirty (WP)";
                        else
                            label10.Text = toc_read_done ? "TOC Clean" : "TOC Dirty";
                        label10.Font = toc_read_done ? new Font(DefaultFont, FontStyle.Regular) : new Font(DefaultFont, FontStyle.Bold);*/

                        string discdatastr = String.Format("MD: Disc Data: {0}, {1}, {2}",
                            discerror ? "Error" : "OK",
                            writeprotected ? "WP" : "no WP",
                            recordable ? "Pre-Mastered" : "Recordable");

#if LOGGING
                        AppendLog(discdatastr);
#else
                        if (!lastdiscstatusstr.Equals(discdatastr))
                            AppendLog(discdatastr);
#endif
                        lastdiscstatusstr = discdatastr;


                    }

                    // 7.13 MODEL NAME
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x22)
                    {
                        string modelname = TrimNonAscii(DecodeAscii(ref ArrRep, 6));
                        AppendLog("MD: Model is {0}", modelname);
                        groupBox1.Text = "Sony " + modelname;
                    }

                    // 7.14 REC DATE DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x24)
                    {
                        byte TrackNo = ArrRep[6];
                        byte Year = ArrRep[7];
                        byte Month = ArrRep[8];
                        byte Day = ArrRep[9];
                        byte Hour = ArrRep[10];
                        byte Min = ArrRep[11];
                        byte Sec = ArrRep[12];

                        // assume anything with a year from the future is from 19xx, most likely recordings are from 20xx
                        int yearprefix = 19;
                        if (Year < (DateTime.Now.Year) - 2000)
                            yearprefix = 20;

                        // a valid day/month starts at 1, so if zero the data is invalid
                        if (Month > 0 && Day > 0)
                        {
                            string recdatestr = String.Format("MD: Track {0} was recorded at time {7}{1:00}-{2:00}-{3:00}T{4:00}:{5:00}:{6:00}", TrackNo, Year, Month, Day, Hour, Min, Sec, yearprefix);
                            if (!recdatestr.Equals(lastrecdatestr))
                                AppendLog(recdatestr);

                            label12_timestamp.Text = String.Format("T{7} Recorded: {6}{0:00}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}", Year, Month, Day, Hour, Min, Sec, yearprefix, TrackNo);

                            lastrecdatestr = recdatestr;
                        }
                        else
                            label12_timestamp.Text = "Recorded: N/A";


                    }

                    // 7.15 DISC NAME
                    if (ArrRep[4] == 0x20 && (ArrRep[5] == 0x48 || ArrRep[5] == 0x49))
                    {
                        byte Segment = ArrRep[6];
                        if (ArrRep[5] == 0x48)
                            discname = TrimNonAscii(DecodeAscii(ref ArrRep, 7));
                        else
                            discname += TrimNonAscii(DecodeAscii(ref ArrRep, 7));
                        label7_disctitle.Text = String.Format("{0}", discname);
                        AppendLog("MD: Disc Title/{1} is: {0}", discname, Segment);
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
                            AppendLog("MD: Track {2}/{3} part {1}: {0}", sb.ToString(), (ArrRep[5] == 0x4A) ? "1" : Segment.ToString(), _infocounter, _lasttrackno);
                        }
                        else
                            AppendLog("MD: Track segment {1}: {0}", currenttrackname, Segment);

                        if (_inforequest)
                        {
                            timer_Poll_GetInfo.Stop();
                            timer_Poll_GetInfo.Start();
                        }

                        // we might be able to update the GUI title field now?
                        UpdateTrackTitle();
                        UpdateDataGrid();
                    }

                    // 7.17 ALL NAME END
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x4C)
                    {
                        _infocounter = -1;
                        AppendLog("MD: All names received.");

                        // if we got this and we're waiting for it, trigger the timer
                        if (_inforequest)
                        {
                            timer_Poll_GetInfo.Stop();
                            timer_Poll_GetInfo.Interval = 10;
                            timer_Poll_GetInfo.Start();
                        }

                        UpdateDataGrid();
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

                        decimal completionratio = (decimal)ts_elapsed.Ticks / (decimal)(ts_track.Ticks + 1);

                        if (ts_track.Ticks == 0 || ts_elapsed.Ticks == 0)
                            progressBar1.Value = 0;
                        else
                            progressBar1.Value = (int)Math.Min(completionratio * 1000, 1000);
#if LOGGING
                        AppendLog("MD: Track {0} elapsed time is {1:00}:{2:00}", TrackNo, Min, Sec);
#endif
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
                        {
                            _remainingrecordtime = new TimeSpan(0, Min, Sec);
                            AppendLog("MD: Record remaining time is {0:00}:{1:00}", Min, Sec);
                        }
                    }

                    // 7.20 NAME REMAIN
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x55)
                    {
                        byte TrackNo = ArrRep[7];
                        byte RemainH = ArrRep[8];
                        byte RemainL = ArrRep[9];
                        int remainingbytes = RemainH << 8 | RemainL;
#if LOGGING
                        AppendLog("MD: Track {0} maximum name size is {1}", TrackNo, remainingbytes);
#endif
                    }

                    // 7.21 TOC DATA
                    if (ArrRep[4] == 0x20 && ArrRep[5] == 0x60)
                    {
                        byte FirstTrackNo = ArrRep[7];
                        byte LastTrackNo = ArrRep[8];


                        // call this since we are now sure what the last track is
                        ReceivedPlayingTrack(0, tracknounchanged: true);

                        byte Min = ArrRep[9];
                        byte Sec = ArrRep[10];
                        _disclength = new TimeSpan(0, Min, Sec);

                        // only log if a change
                        if ((FirstTrackNo != _firsttrackno) || (LastTrackNo != _lasttrackno))
                            AppendLog("MD: First {0}, last {1}. Time {2:00}:{3:00}", FirstTrackNo, LastTrackNo, Min, Sec);
                        label4.Text = String.Format("{0} tracks ({1}-{2}; {3:00}:{4:00}; {5:00}:{6:00} remaining)",
                            1 + LastTrackNo - FirstTrackNo,
                            FirstTrackNo, LastTrackNo,
                            Min, Sec,
                            (int)_remainingrecordtime.TotalMinutes, _remainingrecordtime.Seconds);

                        _lasttrackno = LastTrackNo;
                        _firsttrackno = FirstTrackNo;

                        if (FirstTrackNo != 0 && LastTrackNo != 0)
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

                            TimeSpan ts;
                            bool tracklengthpresent = tracklengths.TryGetValue(_currentrack, out ts);

                            if ((Min != (byte)ts.TotalMinutes) || (Sec != (byte)ts.Seconds))
                                AppendLog("MD: Track {2} length {0:00}:{1:00}", Min, Sec, _currentrack);

                            TimeSpan newts = new TimeSpan(0, Min, Sec);
                            // update index if already present
                            if (tracklengthpresent)
                                tracklengths[_currentrack] = newts;
                            else
                                tracklengths.Add(_currentrack, newts);

                            UpdateDataGridBold(_currentrack);

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
                        progressBar1.Value = 0;
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
    }
}
