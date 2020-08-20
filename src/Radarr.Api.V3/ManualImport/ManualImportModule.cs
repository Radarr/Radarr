using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.MovieImport.Manual;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.ManualImport
{
    public class ManualImportModule : RadarrRestModule<ManualImportResource>
    {
        private readonly IManualImportService _manualImportService;

        public ManualImportModule(IManualImportService manualImportService)
            : base("/manualimport")
        {
            _manualImportService = manualImportService;

            GetResourceAll = GetMediaFiles;
            Post("/",  x => ReprocessItems());
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folder = (string)Request.Query.folder;
            var downloadId = (string)Request.Query.downloadId;
            var filterExistingFiles = Request.GetBooleanQueryParameter("filterExistingFiles", true);
            var movieId = Request.GetNullableIntegerQueryParameter("movieId", null);

            return _manualImportService.GetMediaFiles(folder, downloadId, movieId, filterExistingFiles).ToResource().Select(AddQualityWeight).ToList();
        }

        private object ReprocessItems()
        {
            var items = Request.Body.FromJson<List<ManualImportReprocessResource>>();

            foreach (var item in items)
            {
                var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.MovieId);

                item.Movie = processedItem.Movie.ToResource();
                item.Rejections = processedItem.Rejections;
                item.Languages = processedItem.Languages;
            }

            return items;
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
