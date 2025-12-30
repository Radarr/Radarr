using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.TV;
using Radarr.Http.REST;

namespace Radarr.Api.V3.TVShow
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S6964", Justification = "Follows existing resource patterns - value types validated by FluentValidation")]
    public class EpisodeResource : RestResource
    {
        public int TVShowId { get; set; }
        public int SeasonId { get; set; }
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

        public bool UnverifiedSceneNumbering { get; set; }
        public int? EpisodeFileId { get; set; }
        public bool Monitored { get; set; }
        public bool HasFile { get; set; }

        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public int? AuthorId { get; set; }
        public int? BookSeriesId { get; set; }

        public TVShowResource TVShow { get; set; }
        public EpisodeFileResource EpisodeFile { get; set; }
    }

    public static class EpisodeResourceMapper
    {
        public static EpisodeResource ToResource(this Episode model, bool includeShow = false, bool includeFile = false)
        {
            if (model == null)
            {
                return null;
            }

            return new EpisodeResource
            {
                Id = model.Id,
                TVShowId = model.TVShowId ?? 0,
                SeasonId = model.SeasonId ?? 0,
                SeasonNumber = model.SeasonNumber,
                EpisodeNumber = model.EpisodeNumber,
                AbsoluteEpisodeNumber = model.AbsoluteEpisodeNumber,
                SceneSeasonNumber = model.SceneSeasonNumber,
                SceneEpisodeNumber = model.SceneEpisodeNumber,
                SceneAbsoluteEpisodeNumber = model.SceneAbsoluteEpisodeNumber,
                Title = model.Title,
                Overview = model.Overview,
                AirDate = model.AirDate,
                AirDateUtc = model.AirDateUtc,
                Runtime = model.Runtime,
                UnverifiedSceneNumbering = model.UnverifiedSceneNumbering,
                EpisodeFileId = model.EpisodeFileId,
                Monitored = model.Monitored,
                HasFile = model.EpisodeFileId.HasValue && model.EpisodeFileId.Value > 0,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                Added = model.Added,
                Tags = model.Tags,
                LastSearchTime = model.LastSearchTime,
                AuthorId = model.AuthorId,
                BookSeriesId = model.BookSeriesId
            };
        }

        public static Episode ToModel(this EpisodeResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Episode
            {
                Id = resource.Id,
                TVShowId = resource.TVShowId,
                SeasonId = resource.SeasonId,
                SeasonNumber = resource.SeasonNumber,
                EpisodeNumber = resource.EpisodeNumber,
                AbsoluteEpisodeNumber = resource.AbsoluteEpisodeNumber,
                SceneSeasonNumber = resource.SceneSeasonNumber,
                SceneEpisodeNumber = resource.SceneEpisodeNumber,
                SceneAbsoluteEpisodeNumber = resource.SceneAbsoluteEpisodeNumber,
                Title = resource.Title,
                Overview = resource.Overview,
                AirDate = resource.AirDate,
                AirDateUtc = resource.AirDateUtc,
                Runtime = resource.Runtime,
                UnverifiedSceneNumbering = resource.UnverifiedSceneNumbering,
                EpisodeFileId = resource.EpisodeFileId,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                Added = resource.Added,
                Tags = resource.Tags,
                LastSearchTime = resource.LastSearchTime,
                AuthorId = resource.AuthorId,
                BookSeriesId = resource.BookSeriesId
            };
        }

        public static List<EpisodeResource> ToResource(this IEnumerable<Episode> models, bool includeShow = false, bool includeFile = false)
        {
            return models.Select(m => m.ToResource(includeShow, includeFile)).ToList();
        }
    }
}
