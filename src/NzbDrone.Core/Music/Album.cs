using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Music
{
    public class Album : ModelBase
    {
        public Album()
        {
            Genres = new List<string>();
            Images = new List<MediaCover.MediaCover>();
            Media = new List<Medium>();
            Releases = new List<AlbumRelease>();
            CurrentRelease = new AlbumRelease();
            Ratings = new Ratings();
        }

        public const string RELEASE_DATE_FORMAT = "yyyy-MM-dd";

        public string ForeignAlbumId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; }
        public string Disambiguation { get; set; }
        public string CleanTitle { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<string> Label { get; set; }
        public int ProfileId { get; set; }
        public int Duration { get; set; }
        public List<Track> Tracks { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public List<Medium> Media { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime? LastDiskSync { get; set; }
        public DateTime Added { get; set; }
        public String AlbumType { get; set; }
        public List<SecondaryAlbumType> SecondaryTypes { get; set; }
        //public string ArtworkUrl { get; set; }
        //public string Explicitness { get; set; }
        public AddArtistOptions AddOptions { get; set; }
        public Artist Artist { get; set; }
        public Ratings Ratings { get; set; }
        public List<AlbumRelease> Releases { get; set; }
        public AlbumRelease CurrentRelease { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignAlbumId, Title.NullSafe());
        }

        public void ApplyChanges(Album otherAlbum)
        {

            ForeignAlbumId = otherAlbum.ForeignAlbumId;

            Tracks = otherAlbum.Tracks;

            ProfileId = otherAlbum.ProfileId;
            AddOptions = otherAlbum.AddOptions;
            Monitored = otherAlbum.Monitored;
            CurrentRelease = otherAlbum.CurrentRelease;

        }
    }
}
