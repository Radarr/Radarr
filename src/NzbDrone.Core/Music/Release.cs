using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Music
{
    public class AlbumRelease : ModelBase, IEquatable<AlbumRelease>
    {
        public AlbumRelease()
        {
            OldForeignReleaseIds = new List<string>();
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
        public LazyLoaded<Album> Album { get; set; }
        public LazyLoaded<List<Track>> Tracks { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignReleaseId, Title.NullSafe());
        }

        public bool Equals (AlbumRelease other)
        {
            if (other == null)
            {
                return false;
            }

            if (Id == other.Id &&
                AlbumId == other.AlbumId &&
                ForeignReleaseId == other.ForeignReleaseId &&
                (OldForeignReleaseIds?.SequenceEqual(other.OldForeignReleaseIds) ?? true) &&
                Title == other.Title &&
                Status == other.Status &&
                Duration == other.Duration &&
                (Label?.SequenceEqual(other.Label) ?? true) &&
                Disambiguation == other.Disambiguation &&
                (Country?.SequenceEqual(other.Country) ?? true) &&
                ReleaseDate == other.ReleaseDate &&
                ((Media == null && other.Media == null) || (Media?.ToJson() == other.Media?.ToJson())) &&
                TrackCount == other.TrackCount &&
                Monitored == other.Monitored)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as AlbumRelease;
            if (other == null)
            {
                return false;
            }
            else
            {
                return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id;
                hash = hash * 23 + AlbumId;
                hash = hash * 23 + ForeignReleaseId.GetHashCode();
                hash = hash * 23 + OldForeignReleaseIds?.GetHashCode() ?? 0;
                hash = hash * 23 + Title?.GetHashCode() ?? 0;
                hash = hash * 23 + Status?.GetHashCode() ?? 0;
                hash = hash * 23 + Duration;
                hash = hash * 23 + Label?.GetHashCode() ?? 0;
                hash = hash * 23 + Disambiguation?.GetHashCode() ?? 0;
                hash = hash * 23 + Country?.GetHashCode() ?? 0;
                hash = hash * 23 + ReleaseDate.GetHashCode();
                hash = hash * 23 + Media?.GetHashCode() ?? 0;
                hash = hash * 23 + TrackCount;
                hash = hash * 23 + Monitored.GetHashCode();
                return hash;
            }
        }
    }
}
