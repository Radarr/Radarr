using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Qualities;
using Lidarr.Http.Extensions;
using NzbDrone.Core.Music;
using NLog;
using Nancy;
using Lidarr.Http;
using NzbDrone.Core.MediaFiles;

namespace Lidarr.Api.V1.ManualImport
{
    public class ManualImportModule : LidarrRestModule<ManualImportResource>
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly IManualImportService _manualImportService;
        private readonly Logger _logger;

        public ManualImportModule(IManualImportService manualImportService,
                                  IArtistService artistService,
                                  IAlbumService albumService,
                                  IReleaseService releaseService,
                                  Logger logger)
        {
            _artistService = artistService;
            _albumService = albumService;
            _releaseService = releaseService;
            _manualImportService = manualImportService;
            _logger = logger;

            GetResourceAll = GetMediaFiles;
            
            Put["/"] = options =>
                {
                    var resource = Request.Body.FromJson<List<ManualImportResource>>();
                    return UpdateImportItems(resource).AsResponse(HttpStatusCode.Accepted);
                };
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
                items.Add(new ManualImportItem {
                        Id = resource.Id,
                        Path = resource.Path,
                        RelativePath = resource.RelativePath,
                        Name = resource.Name,
                        Size = resource.Size,
                        Artist = resource.Artist == null ? null : _artistService.GetArtist(resource.Artist.Id),
                        Album = resource.Album == null ? null : _albumService.GetAlbum(resource.Album.Id),
                        Release = resource.AlbumReleaseId == 0 ? null : _releaseService.GetRelease(resource.AlbumReleaseId),
                        Quality = resource.Quality,
                        DownloadId = resource.DownloadId,
                        AdditionalFile = resource.AdditionalFile,
                        ReplaceExistingFiles = resource.ReplaceExistingFiles,
                        DisableReleaseSwitching = resource.DisableReleaseSwitching
                    });
            }
            
            return _manualImportService.UpdateItems(items).Select(x => x.ToResource()).ToList();
        }
    }
}
