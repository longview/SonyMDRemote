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
            Tracks = new SortedDictionary<int, MDTrackData>();
            RecordedDate = DateTime.MinValue;
            CopyProtected = false;
            Stereo = true;
            Error = false;
            WriteProtected = false;
            Recordable = true;
        }

        public bool HasLength()
        {
            return Length.Ticks > 0;
        }
        public bool HasRecordedDate()
        {
            return RecordedDate != DateTime.MinValue;
        }

        public bool HasTitle()
        {
            return Title.Length > 0;
        }

        public string Title;
        public TimeSpan Length;
        public TimeSpan RemainingRecordingTime;
        public SortedDictionary<int, MDTrackData> Tracks;
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
