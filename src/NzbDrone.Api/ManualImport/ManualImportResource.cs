using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Movies;
using NzbDrone.Api.REST;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public MovieResource Movie { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this Core.MediaFiles.EpisodeImport.Manual.ManualImportItem model)
        {
            if (model == null) return null;

            return new ManualImportResource
            {
                Id = HashConverter.GetHashInt31(model.Path),

                Path = model.Path,
                RelativePath = model.RelativePath,
                Name = model.Name,
                Size = model.Size,
                Movie = model.Movie.ToResource(),
                Quality = model.Quality,
                //QualityWeight
                DownloadId = model.DownloadId,
                Rejections = model.Rejections
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<Core.MediaFiles.EpisodeImport.Manual.ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
