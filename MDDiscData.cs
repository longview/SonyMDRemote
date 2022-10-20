using System;
using System.Collections.Generic;
using System.IO;
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

        public string ImportHeader;


        public void Serialize(StreamWriter outputfile, string VersionString = "N/A", string ReleaseString = "N/A")
        {

            outputfile.WriteLine(String.Format("Track listing exported on {0} by LA2YUA SonyMDRemote {1} {2}", DateTime.UtcNow.ToString("o"),
                VersionString, ReleaseString));

            outputfile.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}", Title, Length.ToString(), RemainingRecordingTime.ToString(), RecordedDate.ToString()));

            foreach (var track in Tracks)
            {
                outputfile.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}", track.Key, track.Value.Title, track.Value.Length.ToString(), track.Value.RecordedDate.ToString()));
            }

            outputfile.Close();
        }

        public void Import(StreamReader inputfile)
        {
            ImportHeader = inputfile.ReadLine();

            string[] discinfo = inputfile.ReadLine().Split('\t');

            if (discinfo.Length > 0)
                Title = discinfo[0];
            if (discinfo.Length > 1)
                Length = TimeSpan.Parse(discinfo[1]);
            if (discinfo.Length > 2)
                RemainingRecordingTime = TimeSpan.Parse(discinfo[2]);
            if (discinfo.Length > 3)
                RecordedDate = DateTime.Parse(discinfo[3]);
            

            int tracknumber = 0;

            Tracks.Clear();
            int _firsttrack = int.MaxValue;
            string line;
            while ((line = inputfile.ReadLine()) != null)
            {
                
                string[] trackinfo = line.Split('\t');

                // use tracknumber from file if available, but also keep track of the 1-indexed one
                tracknumber++;
                if (trackinfo.Length > 1)
                    tracknumber = int.Parse(trackinfo[0]);

                if (trackinfo.Length == 1)
                    Tracks.Add(tracknumber, new MDTrackData((uint)tracknumber, trackinfo[0]));
                else
                {
                    Tracks.Add(tracknumber, new MDTrackData((uint)tracknumber, trackinfo[1], TimeSpan.Parse(trackinfo[2])));
                    if (trackinfo.Length > 3)
                        DateTime.TryParse(trackinfo[3], out Tracks[tracknumber].RecordedDate);
                    else
                        Tracks[tracknumber].RecordedDate = DateTime.MinValue;
                }
                if (_firsttrack > tracknumber)
                    _firsttrack = tracknumber;
            }

            FirstTrack = _firsttrack;
            LastTrack = tracknumber;

            inputfile.Close();
        }
    }
}
