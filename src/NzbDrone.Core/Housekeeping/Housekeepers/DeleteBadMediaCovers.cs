using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class DeleteBadMediaCovers : IHousekeepingTask
    {
        private readonly IMetadataFileService _metaFileService;
        private readonly IArtistService _artistService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DeleteBadMediaCovers(IMetadataFileService metaFileService,
                                    IArtistService artistService,
                                    IDiskProvider diskProvider,
                                    IConfigService configService,
                                    Logger logger)
        {
            _metaFileService = metaFileService;
            _artistService = artistService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public void Clean()
        {
            if (!_configService.CleanupMetadataImages) return;

            var artists = _artistService.GetAllArtists();
            var imageExtensions = new List<string> { ".jpg", ".png", ".gif" };

            foreach (var artist in artists)
            {
                var images = _metaFileService.GetFilesByArtist(artist.Id)
                    .Where(c => c.LastUpdated > new DateTime(2014, 12, 27) && imageExtensions.Any(x => c.RelativePath.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)));

                foreach (var image in images)
                {
                    try
                    {
                        var path = Path.Combine(artist.Path, image.RelativePath);
                        if (!IsValid(path))
                        {
                            _logger.Debug("Deleting invalid image file " + path);
                            DeleteMetadata(image.Id, path);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't validate image {0}", image.RelativePath);
                    }
                }
            }


            _configService.CleanupMetadataImages = false;
        }

        private void DeleteMetadata(int id, string path)
        {
            _metaFileService.Delete(id);
            _diskProvider.DeleteFile(path);
        }

        private bool IsValid(string path)
        {
            var buffer = new byte[10];

            using (var imageStream = _diskProvider.OpenReadStream(path))
            {
                if (imageStream.Length < buffer.Length) return false;
                imageStream.Read(buffer, 0, buffer.Length);
            }

            var text = System.Text.Encoding.Default.GetString(buffer);

            return !text.ToLowerInvariant().Contains("html");
        }
    }
}
