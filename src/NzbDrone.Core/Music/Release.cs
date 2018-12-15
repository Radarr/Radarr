using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;

namespace NzbDrone.Core.Music
{
    public class AlbumRelease : ModelBase
    {
        // These correspond to columns in the AlbumReleases table
        public int AlbumId { get; set; }
        public string ForeignReleaseId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public List<string> Label { get; set; }
        public string Disambiguation { get; set; }
        public List<string> Country { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<Medium> Media { get; set; }
        public int TrackCount { get; set; }
        public bool Monitored { get; set; }

        // These are dynamically queried from other tables
        public LazyLoaded<Album> Album { get; set; }
        public LazyLoaded<List<Track>> Tracks { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignReleaseId, Title.NullSafe());
        }
    }
}
