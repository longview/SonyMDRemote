using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonyMDRemote
{
    public class MDDiscData
    {
        public MDDiscData()
        {
            Title = "";
            Length = TimeSpan.Zero;
            RemainingRecordingTime = TimeSpan.Zero;
            FirstTrack = 0;
            LastTrack = 0;
            Tracks = new Dictionary<int, MDTrackData>(30); // most discs have less than 30 tracks so start there
            RecordedDate = DateTime.MinValue;
            CopyProtected = false;
            Stereo = true;
            Error = false;
            WriteProtected = false;
            Recordable = true;
        }

        public string Title;
        public TimeSpan Length;
        public TimeSpan RemainingRecordingTime;
        public IDictionary<int, MDTrackData> Tracks;
        public DateTime RecordedDate;
        public int FirstTrack;
        public int LastTrack;
        public bool CopyProtected;
        public bool Stereo;
        public bool Error;
        public bool WriteProtected;
        public bool Recordable;
    }
}
