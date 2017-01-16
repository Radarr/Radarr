using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Tv
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Actors = new List<Actor>();
            Tags = new HashSet<int>();
            AlternativeTitles = new List<string>();
        }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public int ProfileId { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public string TitleSlug { get; set; }
        public string Website { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public List<Actor> Actors { get; set; }
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public LazyLoaded<MovieFile> MovieFile { get; set; }
        public int MovieFileId { get; set; }
        public List<string> AlternativeTitles { get; set; }
        public string YouTubeTrailerId{ get; set; }
        public string Studio { get; set; }

        public bool HasFile => MovieFileId > 0;

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ImdbId, Title.NullSafe());
        }
    }

    public class AddMovieOptions : MonitoringOptions
    {
        public bool SearchForMovie { get; set; }
    }
}