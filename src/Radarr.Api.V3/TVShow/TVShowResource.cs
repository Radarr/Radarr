using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.TV;
using Radarr.Http.REST;

namespace Radarr.Api.V3.TVShow
{
    public class TVShowResource : RestResource
    {
        public TVShowResource()
        {
            Monitored = true;
        }

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
        public bool UseSceneNumbering { get; set; }

        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public bool Monitored { get; set; }
        public bool MonitorNewItems { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public List<MediaCover> Images { get; set; }
        public List<SeasonResource> Seasons { get; set; }
        public TVShowStatisticsResource Statistics { get; set; }
    }

    public class TVShowStatisticsResource
    {
        public int SeasonCount { get; set; }
        public int EpisodeCount { get; set; }
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public int PercentOfEpisodes { get; set; }
    }

    public static class TVShowResourceMapper
    {
        public static TVShowResource ToResource(this NzbDrone.Core.TV.TVShow model)
        {
            if (model == null)
            {
                return null;
            }

            return new TVShowResource
            {
                Id = model.Id,
                TvdbId = model.TvdbId,
                TmdbId = model.TmdbId,
                ImdbId = model.ImdbId,
                AniDbId = model.AniDbId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                CleanTitle = model.CleanTitle,
                Overview = model.Overview,
                Network = model.Network,
                Status = model.Status,
                Runtime = model.Runtime,
                AirTime = model.AirTime,
                Certification = model.Certification,
                FirstAired = model.FirstAired,
                Year = model.Year,
                Genres = model.Genres,
                OriginalLanguage = model.OriginalLanguage,
                IsAnime = model.IsAnime,
                SeriesType = model.SeriesType,
                UseSceneNumbering = model.UseSceneNumbering,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                QualityProfileId = model.QualityProfileId,
                SeasonFolder = model.SeasonFolder,
                Monitored = model.Monitored,
                MonitorNewItems = model.MonitorNewItems,
                Tags = model.Tags,
                Added = model.Added,
                LastSearchTime = model.LastSearchTime,
                Images = new List<MediaCover>(),
                Seasons = new List<SeasonResource>()
            };
        }

        public static NzbDrone.Core.TV.TVShow ToModel(this TVShowResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new NzbDrone.Core.TV.TVShow
            {
                Id = resource.Id,
                TvdbId = resource.TvdbId,
                TmdbId = resource.TmdbId,
                ImdbId = resource.ImdbId,
                AniDbId = resource.AniDbId,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                CleanTitle = resource.CleanTitle,
                Overview = resource.Overview,
                Network = resource.Network,
                Status = resource.Status,
                Runtime = resource.Runtime,
                AirTime = resource.AirTime,
                Certification = resource.Certification,
                FirstAired = resource.FirstAired,
                Year = resource.Year,
                Genres = resource.Genres,
                OriginalLanguage = resource.OriginalLanguage,
                IsAnime = resource.IsAnime,
                SeriesType = resource.SeriesType,
                UseSceneNumbering = resource.UseSceneNumbering,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                QualityProfileId = resource.QualityProfileId,
                SeasonFolder = resource.SeasonFolder,
                Monitored = resource.Monitored,
                MonitorNewItems = resource.MonitorNewItems,
                Tags = resource.Tags,
                Added = resource.Added,
                LastSearchTime = resource.LastSearchTime
            };
        }

        public static List<TVShowResource> ToResource(this IEnumerable<NzbDrone.Core.TV.TVShow> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
