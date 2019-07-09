using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using Marr.Data;

namespace NzbDrone.Core.Music
{
    public class Album : ModelBase
    {
        public Album()
        {
            Genres = new List<string>();
            Images = new List<MediaCover.MediaCover>();
            Links = new List<Links>();
            Ratings = new Ratings();
            Artist = new Artist();
            OldForeignAlbumIds = new List<string>();
        }

        public const string RELEASE_DATE_FORMAT = "yyyy-MM-dd";

        // These correspond to columns in the Albums table
        // These are metadata entries
        public int ArtistMetadataId { get; set; }
        public string ForeignAlbumId { get; set; }
        public List<string> OldForeignAlbumIds { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string Disambiguation { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public List<string> Genres { get; set; }
        public String AlbumType { get; set; }
        public List<SecondaryAlbumType> SecondaryTypes { get; set; }
        public Ratings Ratings { get; set; }

        // These are Lidarr generated/config        
        public string CleanTitle { get; set; }
        public int ProfileId { get; set; }
        public bool Monitored { get; set; }
        public bool AnyReleaseOk { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime Added { get; set; }
        public AddArtistOptions AddOptions { get; set; }

        // These are dynamically queried from other tables
        public LazyLoaded<ArtistMetadata> ArtistMetadata { get; set; }
        public LazyLoaded<List<AlbumRelease>> AlbumReleases { get; set; }
        public LazyLoaded<Artist> Artist { get; set; }

        //compatibility properties with old version of Album
        public int ArtistId { get { return Artist?.Value?.Id ?? 0; } set { Artist.Value.Id = value; } }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignAlbumId, Title.NullSafe());
        }

        public void ApplyChanges(Album otherAlbum)
        {
            ForeignAlbumId = otherAlbum.ForeignAlbumId;
            ProfileId = otherAlbum.ProfileId;
            AddOptions = otherAlbum.AddOptions;
            Monitored = otherAlbum.Monitored;
            AnyReleaseOk = otherAlbum.AnyReleaseOk;
        }
    }
}
