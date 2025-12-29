using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.TV;
using Radarr.Http.REST;

namespace Radarr.Api.V3.TVShow
{
    public class EpisodeFileResource : RestResource
    {
        public int TVShowId { get; set; }
        public int SeasonNumber { get; set; }

        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }

        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }

        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public StreamingSource StreamingSource { get; set; }

        public string MediaInfo { get; set; }
    }

    public static class EpisodeFileResourceMapper
    {
        public static EpisodeFileResource ToResource(this EpisodeFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new EpisodeFileResource
            {
                Id = model.Id,
                TVShowId = model.TVShowId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,
                Path = model.Path,
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                ReleaseGroup = model.ReleaseGroup,
                Quality = model.Quality,
                Language = model.Language,
                StreamingSource = model.StreamingSource,
                MediaInfo = model.MediaInfo
            };
        }

        public static EpisodeFile ToModel(this EpisodeFileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new EpisodeFile
            {
                Id = resource.Id,
                TVShowId = resource.TVShowId,
                SeasonNumber = resource.SeasonNumber,
                RelativePath = resource.RelativePath,
                Path = resource.Path,
                Size = resource.Size,
                DateAdded = resource.DateAdded,
                SceneName = resource.SceneName,
                ReleaseGroup = resource.ReleaseGroup,
                Quality = resource.Quality,
                Language = resource.Language,
                StreamingSource = resource.StreamingSource,
                MediaInfo = resource.MediaInfo
            };
        }

        public static List<EpisodeFileResource> ToResource(this IEnumerable<EpisodeFile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
