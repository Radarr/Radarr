using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Manual;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.CustomFormats;
using Radarr.Api.V3.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public MovieResource Movie { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this ManualImportItem model)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = model.CustomFormats;
            var customFormatScore = model.Movie?.QualityProfile?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new ManualImportResource
            {
                Id = HashConverter.GetHashInt31(model.Path),
                Path = model.Path,
                RelativePath = model.RelativePath,
                FolderName = model.FolderName,
                Name = model.Name,
                Size = model.Size,
                Movie = model.Movie.ToResource(0),
                ReleaseGroup = model.ReleaseGroup,
                Quality = model.Quality,
                Languages = model.Languages,
                CustomFormats = customFormats.ToResource(false),
                CustomFormatScore = customFormatScore,

                // QualityWeight
                DownloadId = model.DownloadId,
                IndexerFlags = model.IndexerFlags,
                Rejections = model.Rejections
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
