using NzbDrone.Core.Qualities;
using System;
using System.Linq;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedTrackInfo
    {
        //public int TrackNumber { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string ArtistTitle { get; set; }
        public string AlbumTitle { get; set; }
        public ArtistTitleInfo ArtistTitleInfo { get; set; }
        public string ArtistMBId { get; set; }
        public string AlbumMBId { get; set; }
        public string ReleaseMBId { get; set; }
        public string RecordingMBId { get; set; }
        public string TrackMBId { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }
        public IsoCountry Country { get; set; }
        public uint Year { get; set; }
        public string Label { get; set; }
        public string CatalogNumber { get; set; }
        public string Disambiguation { get; set; }
        public TimeSpan Duration { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public int[] TrackNumbers { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }

        public ParsedTrackInfo()
        {
            TrackNumbers = new int[0];
        }

        public override string ToString()
        {
            string trackString = "[Unknown Track]";

            
            if (TrackNumbers != null && TrackNumbers.Any())
            {
                trackString = string.Format("{0}", string.Join("-", TrackNumbers.Select(c => c.ToString("00"))));
            }

            return string.Format("{0} - {1} - {2}:{3} {4}: {5}", ArtistTitle, AlbumTitle, DiscNumber, trackString, Title, Quality);
        }
    }
}
