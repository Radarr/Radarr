using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using Marr.Data;
using NzbDrone.Common.Extensions;
using System;
using NzbDrone.Common.Serializer;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public class Track : ModelBase, IEquatable<Track>
    {
        public Track()
        {
            OldForeignTrackIds = new List<string>();
            OldForeignRecordingIds = new List<string>();
        }

        // These are model fields
        public string ForeignTrackId { get; set; }
        public List<string> OldForeignTrackIds { get; set; }
        public string ForeignRecordingId { get; set; }
        public List<string> OldForeignRecordingIds { get; set; }
        public int AlbumReleaseId { get; set; }
        public int ArtistMetadataId { get; set; }
        public string TrackNumber { get; set; }
        public int AbsoluteTrackNumber { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public bool Explicit { get; set; }
        public Ratings Ratings { get; set; }
        public int MediumNumber { get; set; }
        public int TrackFileId { get; set; }
        public bool HasFile => TrackFileId > 0;

        // These are dynamically queried from the DB
        public LazyLoaded<AlbumRelease> AlbumRelease { get; set; }
        public LazyLoaded<ArtistMetadata> ArtistMetadata { get; set; }
        public LazyLoaded<TrackFile> TrackFile { get; set; }
        public LazyLoaded<Artist> Artist { get; set; }

        // These are retained for compatibility
        // TODO: Remove set, bodged in because tests expect this to be writable
        public int AlbumId { get { return AlbumRelease.Value?.Album.Value?.Id ?? 0; } set { /* empty */ } }
        public Album Album { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", ForeignTrackId, Title.NullSafe());
        }

        public bool Equals(Track other)
        {
            if (other == null)
            {
                return false;
            }

            if (Id == other.Id &&
                ForeignTrackId == other.ForeignTrackId &&
                (OldForeignTrackIds?.SequenceEqual(other.OldForeignTrackIds) ?? true) &&
                ForeignRecordingId == other.ForeignRecordingId &&
                (OldForeignRecordingIds?.SequenceEqual(other.OldForeignRecordingIds) ?? true) &&
                AlbumReleaseId == other.AlbumReleaseId &&
                ArtistMetadataId == other.ArtistMetadataId &&
                TrackNumber == other.TrackNumber &&
                AbsoluteTrackNumber == other.AbsoluteTrackNumber &&
                Title == other.Title &&
                Duration == other.Duration &&
                Explicit == other.Explicit &&
                Ratings?.ToJson() == other.Ratings?.ToJson() &&
                MediumNumber == other.MediumNumber &&
                TrackFileId == other.TrackFileId)
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

            var other = obj as Track;
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
                hash = hash * 23 + ForeignTrackId.GetHashCode();
                hash = hash * 23 + OldForeignTrackIds?.GetHashCode() ?? 0;
                hash = hash * 23 + ForeignRecordingId.GetHashCode();
                hash = hash * 23 + OldForeignRecordingIds?.GetHashCode() ?? 0;
                hash = hash * 23 + AlbumReleaseId;
                hash = hash * 23 + ArtistMetadataId;
                hash = hash * 23 + TrackNumber?.GetHashCode() ?? 0;
                hash = hash * 23 + AbsoluteTrackNumber;
                hash = hash * 23 + Title?.GetHashCode() ?? 0;
                hash = hash * 23 + Duration;
                hash = hash * 23 + Explicit.GetHashCode();
                hash = hash * 23 + Ratings?.GetHashCode() ?? 0;
                hash = hash * 23 + MediumNumber;
                hash = hash * 23 + TrackFileId;
                return hash;
            }
        }
    }
}
