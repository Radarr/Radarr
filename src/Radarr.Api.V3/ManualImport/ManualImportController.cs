using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Manual;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.CustomFormats;
using Radarr.Api.V3.Movies;
using Radarr.Api.V3.MovieVersions;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.ManualImport
{
    [V3ApiController]
    public class ManualImportController : Controller
    {
        private readonly IManualImportService _manualImportService;
        private readonly IMovieVersionService _movieVersionService;

        public ManualImportController(IManualImportService manualImportService, IMovieVersionService movieVersionService)
        {
            _manualImportService = manualImportService;
            _movieVersionService = movieVersionService;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<ManualImportResource>> GetMediaFiles(string folder, string downloadId, int? movieId, bool filterExistingFiles = true)
        {
            var items = movieId.HasValue && downloadId.IsNullOrWhiteSpace()
                ? _manualImportService.GetMediaFiles(movieId.Value)
                : _manualImportService.GetMediaFiles(folder, downloadId, movieId, filterExistingFiles);

            var versions = new Dictionary<int, List<MovieVersionResource>>();

            return TypedResults.Ok(items.ToResource()
                                        .Select(AddQualityWeight)
                                        .Select(i => AddMovieVersions(i, i.Movie?.Id, versions))
                                        .ToList());
        }

        [HttpPost]
        [Consumes("application/json")]
        public Results<Ok<List<ManualImportReprocessResource>>, BadRequest> ReprocessItems([FromBody] List<ManualImportReprocessResource> items)
        {
            if (items is { Count: 0 })
            {
                throw new BadRequestException("items must be provided");
            }

            var versions = new Dictionary<int, List<MovieVersionResource>>();

            foreach (var item in items)
            {
                var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.MovieId, item.MovieVersionId, item.ReleaseGroup, item.Quality, item.Languages, item.IndexerFlags);

                item.Movie = processedItem.Movie.ToResource(0);
                item.MovieVersionId = processedItem.MovieVersionId;
                item.MovieVersions = GetMovieVersions(processedItem.Movie?.Id, versions);
                item.IndexerFlags = processedItem.IndexerFlags;
                item.Rejections = processedItem.Rejections.Select(r => r.ToResource());
                item.CustomFormats = processedItem.CustomFormats.ToResource(false);
                item.CustomFormatScore = processedItem.CustomFormatScore;

                // Only set the language/quality if they're unknown and languages were returned.
                if (item.Languages?.Count <= 1 && (item.Languages?.SingleOrDefault() ?? Language.Unknown) == Language.Unknown && processedItem.Languages.Any())
                {
                    item.Languages = processedItem.Languages;
                }

                if (item.Quality?.Quality == Quality.Unknown)
                {
                    item.Quality = processedItem.Quality;
                }

                if (item.ReleaseGroup.IsNullOrWhiteSpace())
                {
                    item.ReleaseGroup = processedItem.ReleaseGroup;
                }
            }

            return TypedResults.Ok(items);
        }

        // The versions of the item's movie, so the UI can offer to pick one without another request
        private ManualImportResource AddMovieVersions(ManualImportResource item, int? movieId, Dictionary<int, List<MovieVersionResource>> cache)
        {
            item.MovieVersions = GetMovieVersions(movieId, cache);

            return item;
        }

        private List<MovieVersionResource> GetMovieVersions(int? movieId, Dictionary<int, List<MovieVersionResource>> cache)
        {
            if (!movieId.HasValue || movieId.Value == 0)
            {
                return new List<MovieVersionResource>();
            }

            if (!cache.TryGetValue(movieId.Value, out var versions))
            {
                versions = _movieVersionService.GetByMovieId(movieId.Value).ToResource();
                cache[movieId.Value] = versions;
            }

            return versions;
        }

        private ManualImportResource AddQualityWeight(ManualImportResource item)
        {
            if (item.Quality != null)
            {
                item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
                item.QualityWeight += item.Quality.Revision.Real * 10;
                item.QualityWeight += item.Quality.Revision.Version;
            }

            return item;
        }
    }
}
