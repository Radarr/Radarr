using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.TV
{
    public class TVShow : ModelBase
    {
        public TVShow()
        {
            Tags = new HashSet<int>();
            Genres = new List<string>();
        }

        public int? TvdbId { get; set; }
        public int? TmdbId { get; set; }
        public string ImdbId { get; set; }
        public int? AniDbId { get; set; }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string CleanTitle { get; set; }
        public string Overview { get; set; }
        public string Network { get; set; }
        public TVShowStatus Status { get; set; }
        public int? Runtime { get; set; }
        public string AirTime { get; set; }
        public string Certification { get; set; }
        public DateTime? FirstAired { get; set; }
        public int Year { get; set; }
        public List<string> Genres { get; set; }
        public string OriginalLanguage { get; set; }

        public bool IsAnime { get; set; }
        public SeriesType SeriesType { get; set; }
        public bool UseSceneNumbering { get; set; }

        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public bool Monitored { get; set; }
        public bool MonitorNewItems { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Year})";
        }
    }
}
