using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class Artist : ModelBase
    {
        public Artist()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            //Members = new List<Person>(); // Artist Band Member? (NOTE: This should be per album)
            Albums = new List<Album>();
            Tags = new HashSet<int>();

        }

        public int ItunesId { get; set; }
        public string ArtistName { get; set; }
        public string ArtistSlug { get; set; }
        public string CleanTitle { get; set; }
        public bool Monitored { get; set; }
        public bool AlbumFolder { get; set; }
        public bool ArtistFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime? LastDiskSync { get; set; }
        
        public int Status { get; set; } // TODO: Figure out what this is, do we need it? 
        public string Path { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public int QualityProfileId { get; set; }

        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public int ProfileId { get; set; }
        public List<Album> Albums { get; set; }
        public HashSet<int> Tags { get; set; }
        
        public AddSeriesOptions AddOptions { get; set; }

        //public string SortTitle { get; set; }
        //public SeriesStatusType Status { get; set; }
        //public int Runtime { get; set; }
        //public SeriesTypes SeriesType { get; set; }
        //public string Network { get; set; }
        //public bool UseSceneNumbering { get; set; }
        //public string TitleSlug { get; set; }
        //public int Year { get; set; }
        //public Ratings Ratings { get; set; }
        //public List<Actor> Actors { get; set; } // MOve to album?
        //public string Certification { get; set; }
        //public DateTime? FirstAired { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ItunesId, ArtistName.NullSafe());
        }

        public void ApplyChanges(Artist otherArtist)
        {

            ItunesId = otherArtist.ItunesId;
            ArtistName = otherArtist.ArtistName;
            ArtistSlug = otherArtist.ArtistSlug;
            CleanTitle = otherArtist.CleanTitle;
            Monitored = otherArtist.Monitored;
            AlbumFolder = otherArtist.AlbumFolder;
            LastInfoSync = otherArtist.LastInfoSync;
            Images = otherArtist.Images;
            Path = otherArtist.Path;
            Genres = otherArtist.Genres;
            RootFolderPath = otherArtist.RootFolderPath;
            Added = otherArtist.Added;
            Profile = otherArtist.Profile;
            ProfileId = otherArtist.ProfileId;
            Albums = otherArtist.Albums;
            Tags = otherArtist.Tags;
            ArtistFolder = otherArtist.ArtistFolder;
            AddOptions = otherArtist.AddOptions;


        //TODO: Implement
        ItunesId = otherArtist.ItunesId;

            Albums = otherArtist.Albums;
            Path = otherArtist.Path;
            ProfileId = otherArtist.ProfileId;

            AlbumFolder = otherArtist.AlbumFolder;
            Monitored = otherArtist.Monitored;

            //SeriesType = otherArtist.SeriesType;
            RootFolderPath = otherArtist.RootFolderPath;
            Tags = otherArtist.Tags;
            AddOptions = otherArtist.AddOptions;

        }
    }
}
