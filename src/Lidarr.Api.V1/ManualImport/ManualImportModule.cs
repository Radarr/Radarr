using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Qualities;
using Lidarr.Http.Extensions;
using NzbDrone.SignalR;
using NzbDrone.Core.Music;
using NLog;
using Nancy;

namespace Lidarr.Api.V1.ManualImport
{
    public class ManualImportModule : ManualImportModuleWithSignalR
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;

        public ManualImportModule(IManualImportService manualImportService,
                                  IArtistService artistService,
                                  IAlbumService albumService,
                                  IReleaseService releaseService,
                                  IBroadcastSignalRMessage signalRBroadcaster,
                                  Logger logger)
        : base(manualImportService, signalRBroadcaster, logger)
        {
            _artistService = artistService;
            _albumService = albumService;
            _releaseService = releaseService;

            GetResourceAll = GetMediaFiles;
            
            Put["/"] = options =>
                {
                    var resource = Request.Body.FromJson<List<ManualImportResource>>();
                    UpdateImportItems(resource);
                    return GetManualImportItems(resource.Select(x => x.Id)).AsResponse(HttpStatusCode.Accepted);
                };
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folder = (string)Request.Query.folder;
            var downloadId = (string)Request.Query.downloadId;
            var filterExistingFiles = Request.GetBooleanQueryParameter("filterExistingFiles", true);

            return _manualImportService.GetMediaFiles(folder, downloadId, filterExistingFiles).ToResource().Select(AddQualityWeight).ToList();
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

        private void UpdateImportItems(List<ManualImportResource> resources)
        {
            var items = new List<ManualImportItem>();
            foreach (var resource in resources)
            {
                items.Add(new ManualImportItem {
                        Id = resource.Id,
                        Path = resource.Path,
                        RelativePath = resource.RelativePath,
                        FolderName = resource.FolderName,
                        Name = resource.Name,
                        Size = resource.Size,
                        Artist = resource.Artist == null ? null : _artistService.GetArtist(resource.Artist.Id),
                        Album = resource.Album == null ? null : _albumService.GetAlbum(resource.Album.Id),
                        Release = resource.AlbumReleaseId == 0 ? null : _releaseService.GetRelease(resource.AlbumReleaseId),
                        Quality = resource.Quality,
                        Language = resource.Language,
                        DownloadId = resource.DownloadId
                    });
            }
            
            //recalculate import and broadcast
            _manualImportService.UpdateItems(items);
        }
    }
}
