using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface ICleanMetadataService
    {
        void Clean(Artist artist);
    }

    public class CleanExtraFileService : ICleanMetadataService
    {
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public CleanExtraFileService(IMetadataFileService metadataFileService,
                                    IDiskProvider diskProvider,
                                    Logger logger)
        {
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void Clean(Artist artist)
        {
            _logger.Debug("Cleaning missing metadata files for artist: {0}", artist.Name);

            var metadataFiles = _metadataFileService.GetFilesByArtist(artist.Id);

            foreach (var metadataFile in metadataFiles)
            {
                if (!_diskProvider.FileExists(Path.Combine(artist.Path, metadataFile.RelativePath)))
                {
                    _logger.Debug("Deleting metadata file from database: {0}", metadataFile.RelativePath);
                    _metadataFileService.Delete(metadataFile.Id);
                }
            }
        }
    }
}
