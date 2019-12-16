using NzbDrone.Core.MediaFiles;
using Marr.Data;
using NzbDrone.Common.Extensions;
using System.Collections.Generic;
using Equ;

namespace NzbDrone.Core.Music
{
    public class Track : Entity<Track>
    {
        public Track()
        {
            OldForeignTrackIds = new List<string>();
            OldForeignRecordingIds = new List<string>();
            Ratings = new Ratings();
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

        [MemberwiseEqualityIgnore]
        public bool HasFile => TrackFileId > 0;

        // These are dynamically queried from the DB
        [MemberwiseEqualityIgnore]
        public LazyLoaded<AlbumRelease> AlbumRelease { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<ArtistMetadata> ArtistMetadata { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<TrackFile> TrackFile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Artist> Artist { get; set; }

        // These are retained for compatibility
        // TODO: Remove set, bodged in because tests expect this to be writable
        [MemberwiseEqualityIgnore]
        public int AlbumId { get { return AlbumRelease?.Value?.Album?.Value?.Id ?? 0; } set { /* empty */ } }
        [MemberwiseEqualityIgnore]
        public Album Album { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", ForeignTrackId, Title.NullSafe());
        }

        public override void UseMetadataFrom(Track other)
        {
            ForeignTrackId = other.ForeignTrackId;
            OldForeignTrackIds = other.OldForeignTrackIds;
            ForeignRecordingId = other.ForeignRecordingId;
            OldForeignRecordingIds = other.OldForeignRecordingIds;
            TrackNumber = other.TrackNumber;
            AbsoluteTrackNumber = other.AbsoluteTrackNumber;
            Title = other.Title;
            Duration = other.Duration;
            Explicit = other.Explicit;
            Ratings = other.Ratings;
            MediumNumber = other.MediumNumber;
        }

        public override void UseDbFieldsFrom(Track other)
        {
            Id = other.Id;
            AlbumReleaseId = other.AlbumReleaseId;
            ArtistMetadataId = other.ArtistMetadataId;
            TrackFileId = other.TrackFileId;
        }
    }
}
