using System.Collections.Generic;
using System.Linq;
using Nancy;
using NLog;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.ManualImport
{
    public class ManualImportModule : ReadarrRestModule<ManualImportResource>
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IManualImportService _manualImportService;
        private readonly Logger _logger;

        public ManualImportModule(IManualImportService manualImportService,
                                  IArtistService artistService,
                                  IAlbumService albumService,
                                  Logger logger)
        {
            _artistService = artistService;
            _albumService = albumService;
            _manualImportService = manualImportService;
            _logger = logger;

            GetResourceAll = GetMediaFiles;

            Put("/", options =>
                {
                    var resource = Request.Body.FromJson<List<ManualImportResource>>();
                    return ResponseWithCode(UpdateImportItems(resource), HttpStatusCode.Accepted);
                });
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folder = (string)Request.Query.folder;
            var downloadId = (string)Request.Query.downloadId;
            var filter = Request.GetBooleanQueryParameter("filterExistingFiles", true) ? FilterFilesType.Matched : FilterFilesType.None;
            var replaceExistingFiles = Request.GetBooleanQueryParameter("replaceExistingFiles", true);

            return _manualImportService.GetMediaFiles(folder, downloadId, filter, replaceExistingFiles).ToResource().Select(AddQualityWeight).ToList();
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

        private List<ManualImportResource> UpdateImportItems(List<ManualImportResource> resources)
        {
            var items = new List<ManualImportItem>();
            foreach (var resource in resources)
            {
                items.Add(new ManualImportItem
                {
                    Id = resource.Id,
                    Path = resource.Path,
                    Name = resource.Name,
                    Size = resource.Size,
                    Artist = resource.Artist == null ? null : _artistService.GetArtist(resource.Artist.Id),
                    Album = resource.Album == null ? null : _albumService.GetAlbum(resource.Album.Id),
                    Quality = resource.Quality,
                    DownloadId = resource.DownloadId,
                    AdditionalFile = resource.AdditionalFile,
                    ReplaceExistingFiles = resource.ReplaceExistingFiles
                });
            }

            return _manualImportService.UpdateItems(items).Select(x => x.ToResource()).ToList();
        }
    }
}
