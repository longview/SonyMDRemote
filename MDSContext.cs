using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonyMDRemote
{
    public class MDSContext
    {
        // all state data for a MDS series MD recorder
        public MDSContext()
        {
            Disc = new MDDiscData();
            ModelName = "MDS-Exx";
            RemoteEnabled = false;
            PoweredOn = false;
            PlayerState = MDSStatusD1.reserved;
            RepeatState = MDSStatusD2Repeat.REPEAT_OFF;
            AutoPause = false;
            CurrentTrack = 0;
            TOCRead = false;
            TOCDirty = true;
            RecordingPossible = false;
            DigitalInLocked = false;
            RecordingSource = MDSStatusD3Source.reserved;
            CurrentTrackElapsedTime = TimeSpan.Zero;
            CurrentTrackProgress = 0;

            _CommandQueue = new List<MDSTXCommand>(10);
            _CommandQueue_Priority = new List<MDSTXCommand>(10);
            _DiscInfoRequest = 0;
        }

        public MDDiscData Disc;
        public string ModelName;
        public bool RemoteEnabled;
        public bool PoweredOn;
        public MDSStatusD1 PlayerState;
        public MDSStatusD2Repeat RepeatState;
        public MDSStatusD3Source RecordingSource;
        public bool AutoPause;
        public byte CurrentTrack;
        public bool DiscInserted;
        public bool TOCRead;
        public bool TOCDirty;
        public bool RecordingPossible;
        public bool DigitalInLocked;
        public TimeSpan CurrentTrackElapsedTime;
        public TimeSpan CurrentTrackRemainingTime;
        //public MDTrackData CurrentTrackObject;
        public decimal CurrentTrackProgress;


        private List<MDSTXCommand> _CommandQueue_Priority;
        private List<MDSTXCommand> _CommandQueue;
        private int _DiscInfoRequest;
        public System.Windows.Forms.Timer TXTimer;

        public MDTrackData GetCurrentTrackOrNull(int key = -1)
        {
            if (key == -1)
                key = CurrentTrack;
            return Disc.Tracks.ContainsKey(key) ? Disc.Tracks[key] : null;
        }

        public MDTrackData GetCurrentTrackOrEmpty(int key = -1)
        {
            if (key == -1)
                key = CurrentTrack;
            return Disc.Tracks.ContainsKey(key) ? Disc.Tracks[key] : new MDTrackData((uint)key);
        }

        // return true if track existed already
        private bool SetOrUpdateTrack(MDTrackData track)
        {
            if (Disc.Tracks.ContainsKey((int)track.Number))
            {
                Disc.Tracks[(int)track.Number] = track;
                return true;
            }
            else
            {
                Disc.Tracks.Add((int)track.Number, track);
                return false;
            }
        }

        public string GetPlayerStateString()
        {
            if (AutoPause)
                return "("+ this.ToString(PlayerState) +")";
            else
                return this.ToString(PlayerState);
        }

        public string GetCurrentTrackTitleOrDefault()
        {
            if (!Disc.Tracks.ContainsKey(CurrentTrack))
                return "No Title";

            if (Disc.Tracks[CurrentTrack].Title.Length == 0)
                return "No Title";

            return Disc.Tracks[CurrentTrack].Title;
        }

        // parse data received from the MDS player
        // must be pre-framed
        public MDSResponseType ParseRXData(ref byte[] ArrRep)
        {
            
            if (ArrRep.Length < 5)
                return MDSResponseType.Null;
            if (ArrRep[ArrRep.Length - 1] != 0xff)
                return MDSResponseType.Null;

            uint _msgtype = (uint)(ArrRep[4] << 8) | ArrRep[5];
            MDSResponseType messagetype = (MDSResponseType)_msgtype;

            switch (messagetype)
            {
                case MDSResponseType.PowerOn:               PoweredOn = true; break;
                case MDSResponseType.PowerOff:              PoweredOn = false; break;
                    // mecha responses only indicate acknowledge, not anything special
                case MDSResponseType.MechaPlay: break;
                case MDSResponseType.MechaStop: break;
                case MDSResponseType.MechaPause: break;
                case MDSResponseType.MechaREC: break;
                case MDSResponseType.MechaRECPause: break;
                case MDSContext.MDSResponseType.MechaEject: break;
                case MDSResponseType.RemoteOn:              RemoteEnabled = true; break;
                case MDSResponseType.RemoteOff:             RemoteEnabled = false; break;
                case MDSResponseType.InfoModelData:         break;
                case MDSResponseType.InfoStatusData:        HandleInfoStatusData(ref ArrRep); break;
                case MDSResponseType.InfoDiscData:          HandleInfoDiscData(ref ArrRep); break;
                case MDSResponseType.InfoModelName:         ModelName = TrimNonAscii(DecodeAscii(ref ArrRep, 6)); break;
                case MDSResponseType.InfoRecDateData:       HandleInfoRecDateData(ref ArrRep); break;
                case MDSResponseType.InfoDiscName:          HandleInfoDiscName(ref ArrRep); break;
                case MDSResponseType.InfoDiscNameCont:      HandleInfoDiscName(ref ArrRep); break;
                case MDSResponseType.InfoTrackName:         HandleInfoTrackName(ref ArrRep); break;
                case MDSResponseType.InfoTrackNameCont:     HandleInfoTrackName(ref ArrRep); break;
                case MDSResponseType.InfoAllNameEnd:        HandleInfoAllNameEnd(); break;
                case MDSResponseType.InfoElapsedTime:       HandleInfoElapsedTime(ref ArrRep); break;
                case MDSResponseType.InfoRecRemainData:     HandleInfoRecRemainData(ref ArrRep); break;
                case MDSResponseType.InfoNameRemainData:    HandleInfoNameRemainData(ref ArrRep); break;
                case MDSResponseType.InfoTOCData:           HandleInfoTOCData(ref ArrRep); break;
                case MDSResponseType.InfoTrackTimeData:     HandleInfoTrackTimeData(ref ArrRep); break;
                case MDSResponseType.InfoDiscExist: break;
                case MDSResponseType.Info1TrackEnd: break;
                case MDSResponseType.InfoNoDiscName: break;
                case MDSResponseType.InfoNoTOCData: break;
                case MDSResponseType.InfoNoTrackName: break;
                case MDSResponseType.InfoWritePacketReceived: break;
                case MDSResponseType.MessageImpossible: break;
                case MDSResponseType.MessageUndefinedCommand: break;

            }


            return messagetype;
        }

        private void HandleInfoAllNameEnd()
        {
            _DiscInfoRequest = 0;
        }

        private void HandleInfoTrackTimeData(ref byte[] ArrRep)
        {
            if (ArrRep[6] == 0x01 && ArrRep[7] == CurrentTrack)
            {
                byte Min = ArrRep[8];
                byte Sec = ArrRep[9];

                TimeSpan newts = new TimeSpan(0, Min, Sec);

                if (GetCurrentTrackOrEmpty().Length == newts)
                    return;

                MDTrackData tr = GetCurrentTrackOrEmpty();
                tr.Length = newts;
                SetOrUpdateTrack(tr);
            }
        }

        private void HandleInfoTOCData(ref byte[] ArrRep)
        {
            if (ArrRep[7] > ArrRep[8])
                return;

            Disc.FirstTrack = ArrRep[7];
            Disc.LastTrack = ArrRep[8];


            byte Min = ArrRep[9];
            byte Sec = ArrRep[10];
            TimeSpan disclength = new TimeSpan(0, Min, Sec);

            if (disclength != Disc.Length)
                Disc.Length = disclength;

            PruneDiscTracks();
        }

        // Called when we know the track counts, get rid of any track infos that fall outside the valid range
        private void PruneDiscTracks()
        {
            if (!DiscInserted)
                return;
            if (PlayerState == MDSStatusD1.EJECT)
                return;

            List<int> removetracks = new List<int>();
            foreach(var track in Disc.Tracks)
            {
                if (track.Key > Disc.LastTrack)
                    removetracks.Add(track.Key);
                else if (track.Key < Disc.FirstTrack)
                    removetracks.Add(track.Key);
            }

            foreach (int key in removetracks)
            {
                Disc.Tracks.Remove(key);
            }
        }

        private void HandleInfoNameRemainData(ref byte[] ArrRep)
        {
            byte TrackNo = ArrRep[7];
            byte RemainH = ArrRep[8];
            byte RemainL = ArrRep[9];

            if (Disc.Tracks.ContainsKey(TrackNo))
                Disc.Tracks[TrackNo].RemainingNameSpace = (uint)(RemainH << 8 | RemainL);
        }

        private void HandleInfoRecRemainData(ref byte[] ArrRep)
        {
            byte Min = ArrRep[7];
            byte Sec = ArrRep[8];

            if (Min != (int)Disc.RemainingRecordingTime.TotalMinutes || Sec != Disc.RemainingRecordingTime.Seconds)
            {
                Disc.RemainingRecordingTime = new TimeSpan(0, Min, Sec);
            }
        }

        private void HandleInfoElapsedTime(ref byte[] ArrRep)
        {
            CurrentTrack = ArrRep[6];
            byte Min = ArrRep[8];
            byte Sec = ArrRep[9];

            CurrentTrackElapsedTime = new TimeSpan(0, Min, Sec);

            if (!GetCurrentTrackOrEmpty().HasLength())
            {
                CurrentTrackRemainingTime = TimeSpan.Zero;
                CurrentTrackProgress = 0;
                return;
            }

            CurrentTrackRemainingTime = Disc.Tracks[CurrentTrack].Length - CurrentTrackElapsedTime;
            CurrentTrackProgress = (decimal)CurrentTrackElapsedTime.Ticks / (decimal)(Disc.Tracks[CurrentTrack].Length.Ticks);
        }

        private void HandleInfoTrackName(ref byte[] ArrRep)
        {
            byte Segment = ArrRep[6];
            string currenttrackname = TrimNonAscii(DecodeAscii(ref ArrRep, 7));
            //if (_DiscInfoRequest < 0)
                //return;
            if (ArrRep[5] == 0x4A)
            {
                _DiscInfoRequest++;
                if (!Disc.Tracks.ContainsKey(_DiscInfoRequest))
                    Disc.Tracks.Add(_DiscInfoRequest, new MDTrackData((uint)_DiscInfoRequest));
                else
                    Disc.Tracks[_DiscInfoRequest].Title = "";
            }

            Disc.Tracks[_DiscInfoRequest].Title += currenttrackname;
        }

        private void HandleInfoDiscName(ref byte[] ArrRep)
        {
            byte Segment = ArrRep[6];
            if (ArrRep[5] == 0x48)
                Disc.Title = TrimNonAscii(DecodeAscii(ref ArrRep, 7));
            else
                Disc.Title += TrimNonAscii(DecodeAscii(ref ArrRep, 7));
        }

        private void HandleInfoRecDateData(ref byte[] ArrRep)
        {
            if (ArrRep.Length < 13)
                return;

            byte TrackNo = ArrRep[6];
            byte Year = ArrRep[7];
            byte Month = ArrRep[8];
            byte Day = ArrRep[9];
            byte Hour = ArrRep[10];
            byte Min = ArrRep[11];
            byte Sec = ArrRep[12];

            // assume anything with a year from the future is from 19xx, most likely recordings are from 20xx
            int fullyear = 1900 + Year;
            if (Year < (DateTime.Now.Year) - 2000)
                fullyear = 2000 + Year;

            // a valid day/month starts at 1, so if zero the data is invalid
            if (Month > 0 && Day > 0)
            {
                DateTime recdate = new DateTime(fullyear, Month, Day, Hour, Min, Sec);
                if (TrackNo == 0)
                    Disc.RecordedDate = recdate;
                else
                {
                    MDTrackData tr = GetCurrentTrackOrEmpty(TrackNo);
                    tr.RecordedDate = recdate;
                    SetOrUpdateTrack(tr);
                }
            }

        }

        private void HandleInfoDiscData(ref byte[] ArrRep)
        {
            byte DiscData = ArrRep[6];

            Disc.Error = IsBitSet(DiscData, 3);
            Disc.WriteProtected = IsBitSet(DiscData, 2);
            Disc.Recordable = IsBitSet(DiscData, 0);
        }

        private void HandleInfoStatusData(ref byte[] ArrRep)
        {
            byte Data1 = ArrRep[6];
            byte Data2 = ArrRep[7];
            byte Data3 = ArrRep[8];
            // 9 is fixed 1


            bool newtrack = CurrentTrack != ArrRep[10];
            
            CurrentTrack = ArrRep[10];


            DiscInserted = !IsBitSet(Data1, 5);
            PoweredOn = !IsBitSet(Data1, 4);

            switch (Data1 & 0x0f)
            {
                case 0x0: PlayerState = MDSStatusD1.STOP;break;
                case 0x1: PlayerState = MDSStatusD1.PLAY;break;
                case 0x2: PlayerState = MDSStatusD1.PAUSE; break;
                case 0x3: PlayerState = MDSStatusD1.EJECT; break;
                case 0x4: PlayerState = MDSStatusD1.REC_PLAY; break;
                case 0x5: PlayerState = MDSStatusD1.REC_PAUSE; break;
                case 0x6: PlayerState = MDSStatusD1.rehearsal; break;
                default: PlayerState = MDSStatusD1.reserved; break;
            }

            TOCRead = IsBitSet(Data2, 7);
            if (DiscInserted && TOCRead)
                TOCDirty = false;
            else
                TOCDirty = true;

            RecordingPossible = IsBitSet(Data2, 5);

            

            if (IsBitSet(Data2, 4) && !IsBitSet(Data2, 3))
                RepeatState = MDSStatusD2Repeat.TRACK_REPEAT;
            else if (!IsBitSet(Data2, 4) && IsBitSet(Data2, 3))
                RepeatState = MDSStatusD2Repeat.ALL_REPEAT;
            else if (!IsBitSet(Data2, 4) && !IsBitSet(Data2, 3))
                RepeatState = MDSStatusD2Repeat.REPEAT_OFF;

            Disc.Stereo = !IsBitSet(Data3, 7);
            Disc.CopyProtected = IsBitSet(Data3, 6);
            DigitalInLocked = IsBitSet(Data3, 5);

            switch (Data3 & 0x07)
            {
                case 0x1:RecordingSource = MDSStatusD3Source.Analog;break;
                case 0x3:RecordingSource = MDSStatusD3Source.Optical;break;
                case 0x5:RecordingSource = MDSStatusD3Source.Coaxial;break;
            }

            // internal state updates
            if (PlayerState == MDSStatusD1.EJECT)
            {
                CurrentTrack = 0;
                Disc.Title = "No Disc";
            }

            // TODO: if leaving EJECT state with a new disc, clear track lengths?


            if (PlayerState == MDSStatusD1.EJECT || PlayerState == MDSStatusD1.STOP)
            {
                CurrentTrackElapsedTime = TimeSpan.Zero;
                CurrentTrackRemainingTime = TimeSpan.Zero;
                CurrentTrackProgress = 0;
            }

            if (newtrack)
            {
                CurrentTrackElapsedTime = TimeSpan.Zero;
                CurrentTrackRemainingTime = TimeSpan.Zero;
                CurrentTrackProgress = 0;
            }
        }

        public enum MDSResponseType
        {
            Null = 0,
            PowerOn = 0x0102,
            PowerOff = 0x0103,
            MechaPlay = 0x0201,
            MechaStop = 0x0202,
            MechaPause = 0x0203,
            MechaREC = 0x0221,
            MechaRECPause = 0x0225,
            MechaEject = 0x0240,
            RemoteOn = 0x1003,
            RemoteOff = 0x1004,
            InfoModelData = 0x2010,
            InfoStatusData = 0x2020,
            InfoDiscData = 0x2021,
            InfoModelName = 0x2022,
            InfoRecDateData = 0x2024,
            InfoDiscName = 0x2048,
            InfoDiscNameCont = 0x2049,
            InfoTrackName = 0x204A,
            InfoTrackNameCont = 0x204B,
            InfoAllNameEnd = 0x204C,
            InfoElapsedTime = 0x2051,
            InfoRecRemainData = 0x2054,
            InfoNameRemainData = 0x2055,
            InfoTOCData = 0x2060,
            InfoTrackTimeData = 0x2062,
            InfoDiscExist = 0x2082,
            Info1TrackEnd = 0x2083,
            InfoNoDiscName = 0x2085,
            InfoNoTrackName = 0x2086,
            InfoWritePacketReceived = 0x2087,
            InfoNoTOCData = 0x2089,
            // skipped dividemode etc.
            MessageUndefinedCommand = 0x4001,
            MessageImpossible = 0x4003
        }

        public string ToString(MDSResponseType messagetype)
        {
            switch (messagetype)
            {
                case MDSResponseType.PowerOn: return "Powered On";
                case MDSResponseType.PowerOff: return "Powered Off";
                // mecha responses only indicate acknowledge, not anything special
                case MDSResponseType.MechaPlay: return "Playing";
                case MDSResponseType.MechaStop: return "Stopping";
                case MDSResponseType.MechaPause: return "Pausing";
                case MDSResponseType.MechaREC: return "Recording";
                case MDSResponseType.MechaRECPause: return "Pausing Recording";
                case MDSResponseType.RemoteOn: return "Remote On";
                case MDSResponseType.RemoteOff: return "Remote Off";
                case MDSResponseType.InfoModelData: return "Model Data";
                case MDSResponseType.InfoStatusData: return "Status Data";
                case MDSResponseType.InfoDiscData: return "Disc Data";
                case MDSResponseType.InfoModelName: return "Model Name";
                case MDSResponseType.InfoRecDateData: return "Rec Date Data";
                case MDSResponseType.InfoDiscName: return "Disc Name";
                case MDSResponseType.InfoDiscNameCont: return "Disc Name Cont.";
                case MDSResponseType.InfoTrackName: return "Track Name";
                case MDSResponseType.InfoTrackNameCont: return "Track Name Continued";
                case MDSResponseType.InfoAllNameEnd: return "All Name End";
                case MDSResponseType.InfoElapsedTime: return "Elapsed Time";
                case MDSResponseType.InfoRecRemainData: return "Recording Remaining Time";
                case MDSResponseType.InfoNameRemainData: return "Track Name Remaining Bytes";
                case MDSResponseType.InfoTOCData: return "TOC Data";
                case MDSResponseType.InfoTrackTimeData: return "Track Time Data";
                case MDSResponseType.InfoDiscExist: return "Disc Exists";
                case MDSResponseType.Info1TrackEnd: return "End Of Track";
                case MDSResponseType.InfoNoDiscName: return "No Disc Name";
                case MDSResponseType.InfoNoTrackName: return "No Track Name";
                case MDSResponseType.InfoWritePacketReceived: return "Write Packet Received";
                case MDSResponseType.MessageImpossible: return "Command Not Possible Or Unsupported";
                case MDSResponseType.MessageUndefinedCommand: return "Undefined Message";
                default: return "Unknown";
            }
        }


        // 7.11 STATUS DATA
        public enum MDSStatusD1
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

        // not very ergonomic but quick to write as an overloaded ToString
        public string ToString(MDSStatusD1 d1)
        {
            switch (d1)
            {
                case MDSStatusD1.STOP: return "Stop";
                case MDSStatusD1.PLAY: return "Play";
                case MDSStatusD1.PAUSE: return "Pause";
                case MDSStatusD1.EJECT: return "Eject";
                case MDSStatusD1.REC_PAUSE: return "Rec. Pause.";
                case MDSStatusD1.REC_PLAY: return "Rec. Play";
                case MDSStatusD1.rehearsal: return "Rehearsal";
            }
            return "Unknown!";
        }

        // 7.11 STATUS DATA addendum
        public enum MDSStatusD2Repeat
        {
            REPEAT_OFF,
            ALL_REPEAT,
            TRACK_REPEAT
        };

        public string ToString(MDSStatusD2Repeat d2)
        {
            switch (d2)
            {
                case MDSStatusD2Repeat.ALL_REPEAT: return "All Repeat";
                case MDSStatusD2Repeat.REPEAT_OFF: return "No Repeat";
                case MDSStatusD2Repeat.TRACK_REPEAT: return "1Tr Repeat";
            }
            return "Unknown!";
        }

        // 7.11 STATUS DATA
        public enum MDSStatusD3Source
        {
            Analog = 1,
            Optical = 3,
            Coaxial = 5,
            reserved = 7
        };

        public string ToString(MDSStatusD3Source d3)
        {
            switch (d3)
            {
                case MDSStatusD3Source.Analog: return "Analog";
                case MDSStatusD3Source.Coaxial: return "Coaxial";
                case MDSStatusD3Source.Optical: return "Optical";
            }
            return "Unknown!";
        }

        // stuff for issuing commands to the MDSes
        public class TransmitData
        {
            public class MDSTXCommand
            {
                public MDSTXCommand() { }
                public MDSMsgType MessageType;
                public byte TrackNumber;
                public bool IsTrackDump;
                public int PostCommandDelay;
            }

            public enum MDSMsgType
            {
                Unknown = 0xffff,
                SetPowerOn = 0x0102,
                SetPowerOff = 0x0103,
                SetRemoteOn = 0x1003,
                SetRemoteOff = 0x1004,
                ReqStatus = 0x2020,
                ReqDiscData = 0x2021,
                ReqModelName = 0x2022,
                ReqTrackRecordDate = 0x2024,
                ReqTOCData = 0x2044,
                ReqTrackTime = 0x2045,
                ReqDiscName = 0x2048,
                ReqDiscAndTrackNames = 0x204c,
                ReqRemainingRecordTime = 0x2054,
                ReqTrackRemainingNameSize = 0x2055,
                WriteDiscName1 = 0x2070,
                WriteDiscName2 = 0x2071,
                WriteTrackName1 = 0x2072,
                WriteTrackName2 = 0x2073,
                Play = 0x0201,
                Stop = 0x0202,
                PlayPause = 0x0203,
                Pause = 0x0206,
                FFW_REW_Off=0x00,
                Rewind = 0x0213,
                FastForward = 0x0214,
                NextTrack = 0x0216,
                PrevTrack = 0x0215,
                RecordArm = 0x0221,
                Eject = 0x0240,
                AutoPauseOn = 0x0281,
                AutoPauseOff = 0x0280,
                RepeatOff = 0x02A0,
                RepeatAll = 0x02A1,
                Repeat1Tr = 0x02A2,
                StartPlayAtTrack = 0x0342,
                PauseAtTrack = 0x0343,
                EnableElapsedTimeTransmit = 0x0710,
                DisableElapsedTimeTransmit = 0x0711
            }

            public static string ToString(MDSMsgType msg)
            {
                switch (msg)
                {
                    case MDSMsgType.AutoPauseOff: return "Auto Pause Off";
                    case MDSMsgType.AutoPauseOn: return "Auto Pause On";
                    case MDSMsgType.DisableElapsedTimeTransmit: return "Disable Elapsed Time Transmit";
                    case MDSMsgType.Eject: return "Eject";
                    case MDSMsgType.EnableElapsedTimeTransmit: return "Enable Elapsed Time Transmit";
                    case MDSMsgType.FastForward: return "Fast Forward";
                    case MDSMsgType.FFW_REW_Off: return "FFW REW OFF";
                    case MDSMsgType.NextTrack: return "Next Track";
                    case MDSMsgType.Pause: return "Pause";
                    case MDSMsgType.PauseAtTrack: return "Pause At Track";
                    case MDSMsgType.Play: return "Play";
                    case MDSMsgType.PlayPause: return "Pause";
                    case MDSMsgType.PrevTrack: return "Previous Track";
                    case MDSMsgType.RecordArm: return "Record Arm";
                    case MDSMsgType.Repeat1Tr: return "Repeat 1 Track";
                    case MDSMsgType.RepeatAll: return "Repeat All";
                    case MDSMsgType.RepeatOff: return "Repeat Off";
                    case MDSMsgType.ReqDiscAndTrackNames: return "Request Disc And Track Names";
                    case MDSMsgType.ReqDiscData: return "Request Disc Data";
                    case MDSMsgType.ReqDiscName: return "Request Disc Name";
                    case MDSMsgType.ReqModelName: return "Request Model Name";
                    case MDSMsgType.ReqRemainingRecordTime: return "Request Remaining Record Time";
                    case MDSMsgType.ReqStatus: return "Request Status";
                    case MDSMsgType.ReqTOCData: return "Request TOC Data";
                    case MDSMsgType.ReqTrackRecordDate: return "Request Track Recording Date";
                    case MDSMsgType.ReqTrackRemainingNameSize: return "Request Track Remaining Name Size";
                    case MDSMsgType.ReqTrackTime: return "Request Track Length";
                    case MDSMsgType.Rewind: return "Rewind";
                    case MDSMsgType.SetPowerOff: return "Set Power Off";
                    case MDSMsgType.SetPowerOn: return "Set Power On";
                    case MDSMsgType.SetRemoteOff: return "Set Remote Off";
                    case MDSMsgType.SetRemoteOn: return "Set Remote On";
                    case MDSMsgType.StartPlayAtTrack: return "Start Playing At Track";
                    case MDSMsgType.Stop: return "Stop";
                    case MDSMsgType.WriteDiscName1: return "Write Disc Name, 1st packet";
                    case MDSMsgType.WriteDiscName2: return "Write Disc Name, continued";
                    case MDSMsgType.WriteTrackName1: return "Write Track Name, 1st packet";
                    case MDSMsgType.WriteTrackName2: return "Write Track Name, continued";
                    default: return "Unknown";
                }
            }

        }


        private bool IsBitSet(byte b, int pos)
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
    }
}
