using NzbDrone.Common.Extensions;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedTrackInfo
    {
        //public int TrackNumber { get; set; }
        public string Title { get; set; }
        public string ArtistTitle { get; set; }
        public string AlbumTitle { get; set; }
        public ArtistTitleInfo ArtistTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int[] TrackNumbers { get; set; }
        //public Language Language { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }

        public ParsedTrackInfo()
        {
            TrackNumbers = new int[0];
        }




        public override string ToString()
        {
            string episodeString = "[Unknown Track]";

            
            if (TrackNumbers != null && TrackNumbers.Any())
            {
                episodeString = string.Format("T{0}", string.Join("-", TrackNumbers.Select(c => c.ToString("00"))));
            }
            

            return string.Format("{0} - {1} {2}", ArtistTitle, episodeString, Quality);
        }
    }
}
