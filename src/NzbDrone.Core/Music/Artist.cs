using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
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
        //public int TvRageId { get; set; }
        //public int TvMazeId { get; set; }
        //public string ImdbId { get; set; }
        public string ArtistName { get; set; }
        public string ArtistSlug { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        //public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        //public int ProfileId { get; set; }
        public bool AlbumFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        //public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        //public SeriesTypes SeriesType { get; set; }
        //public string Network { get; set; }
        //public bool UseSceneNumbering { get; set; }
        //public string TitleSlug { get; set; }
        public string Path { get; set; }
        //public int Year { get; set; }
        //public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        //public List<Actor> Actors { get; set; } // MOve to album?
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? FirstAired { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public int ProfileId { get; set; }

        public List<Album> Albums { get; set; }
        public HashSet<int> Tags { get; set; }
        //public AddSeriesOptions AddOptions { get; set; } // TODO: Learn what this does

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ItunesId, ArtistName.NullSafe());
        }

        public void ApplyChanges(Artist otherArtist)
        {
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
            //AddOptions = otherArtist.AddOptions;
        }
    }
}
