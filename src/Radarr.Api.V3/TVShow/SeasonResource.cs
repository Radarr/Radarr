using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.TV;
using Radarr.Http.REST;

namespace Radarr.Api.V3.TVShow
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S6964", Justification = "Follows existing resource patterns - value types validated by FluentValidation")]
    public class SeasonResource : RestResource
    {
        public int TVShowId { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public SeasonStatisticsResource Statistics { get; set; }
    }

    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S6964", Justification = "Statistics resource - read only")]
    public class SeasonStatisticsResource
    {
        public int EpisodeCount { get; set; }
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public int PercentOfEpisodes { get; set; }
    }

    public static class SeasonResourceMapper
    {
        public static SeasonResource ToResource(this Season model)
        {
            if (model == null)
            {
                return null;
            }

            return new SeasonResource
            {
                Id = model.Id,
                TVShowId = model.TVShowId ?? 0,
                SeasonNumber = model.SeasonNumber,
                Title = model.Title,
                Overview = model.Overview,
                Monitored = model.Monitored
            };
        }

        public static Season ToModel(this SeasonResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Season
            {
                Id = resource.Id,
                TVShowId = resource.TVShowId,
                SeasonNumber = resource.SeasonNumber,
                Title = resource.Title,
                Overview = resource.Overview,
                Monitored = resource.Monitored
            };
        }

        public static List<SeasonResource> ToResource(this IEnumerable<Season> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
