using System;
using System.Collections.Generic;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.MetadataSource.TV
{
    public class TVShowMetadata
    {
        public int TvdbId { get; set; }
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

        public List<string> AlternateTitles { get; set; }
        public List<SeasonMetadata> Seasons { get; set; }
        public List<ActorMetadata> Actors { get; set; }

        public string PosterUrl { get; set; }
        public string FanartUrl { get; set; }
        public string BannerUrl { get; set; }

        public double? Rating { get; set; }
        public int? Votes { get; set; }

        public TVShowMetadata()
        {
            Genres = new List<string>();
            AlternateTitles = new List<string>();
            Seasons = new List<SeasonMetadata>();
            Actors = new List<ActorMetadata>();
        }
    }

    public class SeasonMetadata
    {
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterUrl { get; set; }
        public List<EpisodeMetadata> Episodes { get; set; }

        public SeasonMetadata()
        {
            Episodes = new List<EpisodeMetadata>();
        }
    }

    public class EpisodeMetadata
    {
        public int TvdbId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime? AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int? Runtime { get; set; }
        public string StillUrl { get; set; }
        public double? Rating { get; set; }
    }

    public class ActorMetadata
    {
        public string Name { get; set; }
        public string Character { get; set; }
        public string ImageUrl { get; set; }
        public int? Order { get; set; }
    }
}
