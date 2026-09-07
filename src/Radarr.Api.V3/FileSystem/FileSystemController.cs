using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using Radarr.Http;

namespace Radarr.Api.V3.FileSystem
{
    [V3ApiController]
    public class FileSystemController : Controller
    {
        private readonly IFileSystemLookupService _fileSystemLookupService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;

        public FileSystemController(IFileSystemLookupService fileSystemLookupService,
                                IDiskProvider diskProvider,
                                IDiskScanService diskScanService)
        {
            _fileSystemLookupService = fileSystemLookupService;
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
        }

        [HttpGet]
        public FileSystemResult GetContents(string path, bool includeFiles = false, bool allowFoldersWithoutTrailingSlashes = false)
        {
            return _fileSystemLookupService.LookupContents(path, includeFiles, allowFoldersWithoutTrailingSlashes);
        }

        [HttpGet("type")]
        public FileSystemTypeResource GetEntityType(string path)
        {
            if (_diskProvider.FileExists(path))
            {
                return new FileSystemTypeResource { Type = FileSystemEntityType.File };
            }

            // Return folder even if it doesn't exist on disk to avoid leaking anything from the UI about the underlying system
            return new FileSystemTypeResource { Type = FileSystemEntityType.Folder };
        }

        [HttpGet("mediafiles")]
        public List<FileSystemMediaFileResource> GetMediaFiles(string path)
        {
            if (!_diskProvider.FolderExists(path))
            {
                return new List<FileSystemMediaFileResource>();
            }

            return _diskScanService.GetVideoFiles(path).Select(f => new FileSystemMediaFileResource
            {
                Path = f,
                RelativePath = path.GetRelativePath(f),
                Name = Path.GetFileName(f)
            }).ToList();
        }
    }
}
