using NzbDrone.Common.Extensions;
using System;
using System.Collections.Generic;
using Marr.Data;
using Equ;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public class Album : Entity<Album>
    {
        public Album()
        {
            OldForeignAlbumIds = new List<string>();

            Images = new List<MediaCover.MediaCover>();
            Links = new List<Links>();
            Genres = new List<string>();
            SecondaryTypes = new List<SecondaryAlbumType>();
            Ratings = new Ratings();
            Artist = new Artist();

        }

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
        [MemberwiseEqualityIgnore]
        public AddArtistOptions AddOptions { get; set; }

        // These are dynamically queried from other tables
        [MemberwiseEqualityIgnore]
        public LazyLoaded<ArtistMetadata> ArtistMetadata { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<AlbumRelease>> AlbumReleases { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Artist> Artist { get; set; }

        //compatibility properties with old version of Album
        [MemberwiseEqualityIgnore]
        public int ArtistId { get { return Artist?.Value?.Id ?? 0; } set { Artist.Value.Id = value; } }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignAlbumId, Title.NullSafe());
        }

        public override void UseMetadataFrom(Album other)
        {
            ForeignAlbumId = other.ForeignAlbumId;
            OldForeignAlbumIds = other.OldForeignAlbumIds;
            Title = other.Title;
            Overview = other.Overview.IsNullOrWhiteSpace() ? Overview : other.Overview;
            Disambiguation = other.Disambiguation;
            ReleaseDate = other.ReleaseDate;
            Images = other.Images.Any() ? other.Images : Images;
            Links = other.Links;
            Genres = other.Genres;
            AlbumType = other.AlbumType;
            SecondaryTypes = other.SecondaryTypes;
            Ratings = other.Ratings;
            CleanTitle = other.CleanTitle;
        }

        public override void UseDbFieldsFrom(Album other)
        {
            Id = other.Id;
            ArtistMetadataId = other.ArtistMetadataId;
            ProfileId = other.ProfileId;
            Monitored = other.Monitored;
            AnyReleaseOk = other.AnyReleaseOk;
            LastInfoSync = other.LastInfoSync;
            Added = other.Added;
            AddOptions = other.AddOptions;
        }

        public override void ApplyChanges(Album otherAlbum)
        {
            ForeignAlbumId = otherAlbum.ForeignAlbumId;
            ProfileId = otherAlbum.ProfileId;
            AddOptions = otherAlbum.AddOptions;
            Monitored = otherAlbum.Monitored;
            AnyReleaseOk = otherAlbum.AnyReleaseOk;
        }
    }
}
