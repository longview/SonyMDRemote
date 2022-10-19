using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonyMDRemote
{
    // track data
    public class MDTrackData
    {
        public MDTrackData(string title, uint number)
        {
            Title = title;
            Length = TimeSpan.Zero;
            RecordedDate = DateTime.MinValue;
            Number = number;
        }

        public MDTrackData(string title, uint number, TimeSpan length, DateTime recordeddate)
        {
            Title = title;
            Length = length;
            RecordedDate = recordeddate;
            Number = number;
        }

        public bool HasLength()
        {
            return Length.Ticks > 0;
        }
        public bool HasRecordedDate()
        {
            return RecordedDate == DateTime.MinValue;
        }

        public bool HasTitle()
        {
            return Title.Length > 0;
        }

        public uint Number;
        public string Title;
        public TimeSpan Length;
        public DateTime RecordedDate;
    }
}
