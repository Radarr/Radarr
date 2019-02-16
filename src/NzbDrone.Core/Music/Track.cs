using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using Marr.Data;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public class Track : ModelBase
    {
        public Track()
        {
        }

        // These are model fields
        public string ForeignTrackId { get; set; }
        public string ForeignRecordingId { get; set; }
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
    }
}
