using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Others
{
    public interface IOtherExtraFileRenamer
    {
        void RenameOtherExtraFile(Author author, string path);
    }

    public class OtherExtraFileRenamer : IOtherExtraFileRenamer
    {
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IAuthorService _authorService;
        private readonly IOtherExtraFileService _otherExtraFileService;

        public OtherExtraFileRenamer(IOtherExtraFileService otherExtraFileService,
                                     IAuthorService authorService,
                                     IRecycleBinProvider recycleBinProvider,
                                     IDiskProvider diskProvider,
                                     Logger logger)
        {
            _logger = logger;
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _authorService = authorService;
            _otherExtraFileService = otherExtraFileService;
        }

        public void RenameOtherExtraFile(Author author, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return;
            }

            var relativePath = author.Path.GetRelativePath(path);

            var otherExtraFile = _otherExtraFileService.FindByPath(relativePath);
            if (otherExtraFile != null)
            {
                var newPath = path + "-orig";

                // Recycle an existing -orig file.
                RemoveOtherExtraFile(author, newPath);

                // Rename the file to .*-orig
                _diskProvider.MoveFile(path, newPath);
                otherExtraFile.RelativePath = relativePath + "-orig";
                otherExtraFile.Extension += "-orig";
                _otherExtraFileService.Upsert(otherExtraFile);
            }
        }

        private void RemoveOtherExtraFile(Author author, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return;
            }

            var relativePath = author.Path.GetRelativePath(path);

            var otherExtraFile = _otherExtraFileService.FindByPath(relativePath);
            if (otherExtraFile != null)
            {
                var subfolder = Path.GetDirectoryName(relativePath);
                _recycleBinProvider.DeleteFile(path, subfolder);
            }
        }
    }
}
