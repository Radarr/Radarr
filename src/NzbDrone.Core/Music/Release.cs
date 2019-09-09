using NzbDrone.Common.Extensions;
using System;
using System.Collections.Generic;
using Marr.Data;
using Equ;

namespace NzbDrone.Core.Music
{
    public class AlbumRelease : Entity<AlbumRelease>
    {
        public AlbumRelease()
        {
            OldForeignReleaseIds = new List<string>();
            Label = new List<string>();
            Country = new List<string>();
            Media = new List<Medium>();
        }

        // These correspond to columns in the AlbumReleases table
        public int AlbumId { get; set; }
        public string ForeignReleaseId { get; set; }
        public List<string> OldForeignReleaseIds { get; set; }
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
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Album> Album { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<Track>> Tracks { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignReleaseId, Title.NullSafe());
        }

        public override void UseMetadataFrom(AlbumRelease other)
        {
            ForeignReleaseId = other.ForeignReleaseId;
            OldForeignReleaseIds = other.OldForeignReleaseIds;
            Title = other.Title;
            Status = other.Status;
            Duration = other.Duration;
            Label = other.Label;
            Disambiguation = other.Disambiguation;
            Country = other.Country;
            ReleaseDate = other.ReleaseDate;
            Media = other.Media;
            TrackCount = other.TrackCount;
        }

        public override void UseDbFieldsFrom(AlbumRelease other)
        {
            Id = other.Id;
            AlbumId = other.AlbumId;
            Album = other.Album;
            Monitored = other.Monitored;
        }
    }
}
