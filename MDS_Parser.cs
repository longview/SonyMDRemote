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


                    MDSContext.MDSResponseType messagetype = mdctx.ParseRXData(ref ArrRep);
#if DEBUG
                    AppendLog("MD sent: type {2}, {0}, ASCII: {1}", BitConverter.ToString(ArrRep), TrimNonAscii(System.Text.Encoding.ASCII.GetString(ArrRep)),
                        mdctx.ToString(messagetype));
#endif                   

                    switch (messagetype)
                    {
                        case MDSContext.MDSResponseType.PowerOn:
                        case MDSContext.MDSResponseType.PowerOff:
                            AppendLog("MD: Power is {0}", mdctx.PoweredOn ? "on" : "off");
                            break;
                        case MDSContext.MDSResponseType.MechaPlay: AppendLog("MD: Playing"); break;
                        case MDSContext.MDSResponseType.MechaStop: AppendLog("MD: Stopped"); break;
                        case MDSContext.MDSResponseType.MechaPause: AppendLog("MD: Paused"); break;
                        case MDSContext.MDSResponseType.MechaREC: AppendLog("MD: Recording started"); break;
                        case MDSContext.MDSResponseType.MechaRECPause: AppendLog("MD: Recording paused"); break;
                        case MDSContext.MDSResponseType.MechaEject: AppendLog("MD: Disc ejected"); break;
                        case MDSContext.MDSResponseType.RemoteOn:
                        case MDSContext.MDSResponseType.RemoteOff:
                            AppendLog("MD: Remote is {0}", mdctx.RemoteEnabled ? "on" : "off");
                            break;
                        case MDSContext.MDSResponseType.InfoModelData: break;
                        case MDSContext.MDSResponseType.InfoStatusData:
                            label3_playstatusindicator.Text = mdctx.GetPlayerStateString();

                            switch (mdctx.PlayerState)
                            {
                                case MDSContext.MDSStatusD1.STOP: label3_playstatusindicator.ForeColor = Color.Black; break;
                                case MDSContext.MDSStatusD1.PLAY: label3_playstatusindicator.ForeColor = Color.Green; break;
                                case MDSContext.MDSStatusD1.PAUSE: label3_playstatusindicator.ForeColor = Color.Orange; break;
                                case MDSContext.MDSStatusD1.EJECT: label3_playstatusindicator.ForeColor = Color.Black; break;
                                case MDSContext.MDSStatusD1.REC_PAUSE: label3_playstatusindicator.ForeColor = Color.Red; break;
                                case MDSContext.MDSStatusD1.REC_PLAY: label3_playstatusindicator.ForeColor = Color.OrangeRed; break;
                            }

                            label11.Text = mdctx.ToString(mdctx.RepeatState);
                            ReceivedPlayingTrack();

                            if (mdctx.PlayerState == MDSContext.MDSStatusD1.EJECT)
                                label10.Text = "No Disc";
                            else
                                label10.Text = mdctx.DiscInserted ? "No Disc" : mdctx.TOCDirty ? "TOC Dity" : "TOC Clean";



                            if (mdctx.PlayerState != MDSContext.MDSStatusD1.EJECT)
                            {
                                // request these since we now know the track number
                                Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: _currentrack);
                                Transmit_MDS_Message(MDS_TX_ReqTOCData);
                            }

                            if (mdctx.PlayerState == MDSContext.MDSStatusD1.EJECT)
                            {
                                label12_timestamp.Text = "Recorded: N/A";
                                label7_disctitle.Text = "No Disc";
                            }
                            else
                                label7_disctitle.Text = mdctx.Disc.Title;

                            Transmit_MDS_Message(MDS_TX_ReqTrackTime, tracknumber: mdctx.CurrentTrack);

                            break;
                        case MDSContext.MDSResponseType.InfoDiscData: break; // NOP
                        case MDSContext.MDSResponseType.InfoModelName:
                            groupBox1.Text = "Sony " + mdctx.ModelName;
                            break;
                        case MDSContext.MDSResponseType.InfoRecDateData: break;
                        case MDSContext.MDSResponseType.InfoDiscName:
                        case MDSContext.MDSResponseType.InfoDiscNameCont:
                            if (mdctx.Disc.HasTitle())
                                label7_disctitle.Text = mdctx.Disc.Title.ToString();
                            else
                                label7_disctitle.Text = "No Title";
                            break;
                        case MDSContext.MDSResponseType.InfoTrackName:
                        case MDSContext.MDSResponseType.InfoTrackNameCont:
                        case MDSContext.MDSResponseType.InfoAllNameEnd:
                            UpdateTrackTitle();
                            UpdateDataGrid();
                            break;
                        case MDSContext.MDSResponseType.InfoElapsedTime:
                            ReceivedPlayingTrack();
                            progressBar1.Value = (int)Math.Min(mdctx.CurrentTrackProgress * 1000, 1000);
                            if (checkBox2_Elapsed.Checked)
                            {
                                MDTrackData track = new MDTrackData(0);
                                mdctx.Disc.Tracks.TryGetValue(mdctx.CurrentTrack, out track);
                                if (track == null)
                                    track = new MDTrackData(0);
                                label6.Text = String.Format("{0:00}:{1:00}/{2:00}:{3:00} (r: {4:00}:{5:00})",
                                    (int)mdctx.CurrentTrackElapsedTime.TotalMinutes, mdctx.CurrentTrackElapsedTime.Seconds,
                                                        (int)track.Length.TotalMinutes, track.Length.Seconds,
                                                        (int)mdctx.CurrentTrackRemainingTime.TotalMinutes, mdctx.CurrentTrackRemainingTime.Seconds);
                            }
                                
                            break;
                        case MDSContext.MDSResponseType.InfoRecRemainData: break;
                        case MDSContext.MDSResponseType.InfoNameRemainData: break;
                        case MDSContext.MDSResponseType.InfoTOCData:
                            ReceivedPlayingTrack();

                            label4.Text = String.Format("{0} tracks ({1}-{2}; {3:00}:{4:00}; {5:00}:{6:00} remaining)",
                                    1 + mdctx.Disc.LastTrack - mdctx.Disc.FirstTrack,
                                    mdctx.Disc.FirstTrack, mdctx.Disc.LastTrack,
                                    (int)mdctx.Disc.Length.TotalMinutes, (int)mdctx.Disc.Length.Seconds,
                                    (int)mdctx.Disc.RemainingRecordingTime.TotalMinutes, (int)mdctx.Disc.RemainingRecordingTime.Seconds);

                            if (mdctx.Disc.FirstTrack != 0 && mdctx.Disc.LastTrack != 0)
                            {
                                try
                                {
                                    numericUpDown1.Minimum = mdctx.Disc.FirstTrack;
                                    numericUpDown1.Maximum = mdctx.Disc.LastTrack;
                                    if (numericUpDown1.Value < mdctx.Disc.FirstTrack)
                                        numericUpDown1.Value = mdctx.Disc.FirstTrack;
                                    if (numericUpDown1.Value > mdctx.Disc.LastTrack && mdctx.Disc.LastTrack != 0)
                                        numericUpDown1.Value = mdctx.Disc.LastTrack;

                                    numericUpDown1.Minimum = mdctx.Disc.FirstTrack;
                                    numericUpDown1.Maximum = mdctx.Disc.LastTrack;
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

                            break;
                        case MDSContext.MDSResponseType.InfoTrackTimeData:
                            if (!checkBox2_Elapsed.Checked)
                                label6.Text = GetTrackLenFormatted(mdctx.CurrentTrack);
                            UpdateDataGridBold();
                            break;
                        case MDSContext.MDSResponseType.InfoDiscExist:
                            AppendLog("MD: TOC read, disc ready");
                            break;
                        case MDSContext.MDSResponseType.Info1TrackEnd:
                            AppendLog("MD: Track changed");
                            progressBar1.Value = 0;
                            Transmit_MDS_Message(MDS_TX_ReqStatus);
                            break;
                        case MDSContext.MDSResponseType.InfoNoDiscName:
                            AppendLog("MD: Current disc not named");
                            break;
                        case MDSContext.MDSResponseType.InfoNoTOCData:
                            AppendLog("MD: No TOC data present or no disc inserted");
                            break;
                        case MDSContext.MDSResponseType.InfoNoTrackName:
                            AppendLog("MD: Current track not named");
                            break;
                        case MDSContext.MDSResponseType.InfoWritePacketReceived:
                            AppendLog("MD: Write packet acknowledge");
                            break;
                        case MDSContext.MDSResponseType.MessageImpossible:
                            AppendLog("MD: Reports previous command not possible");
                            break;
                        case MDSContext.MDSResponseType.MessageUndefinedCommand:
                            AppendLog("MD: Unknown command received by MD");
                            break;

                    }
                }

            }
        }
    }
}
