using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IOtherExtraFileService _otherExtraFileService;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _otherExtraFileService = otherExtraFileService;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterAuthorScan(Author author, List<BookFile> bookFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Author author, BookFile bookFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterBookImport(Author author, Book book, string artistFolder, string albumFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Author author, List<BookFile> bookFiles)
        {
            var extraFiles = _otherExtraFileService.GetFilesByArtist(author.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var bookFile in bookFiles)
            {
                var extraFilesForTrackFile = extraFiles.Where(m => m.BookFileId == bookFile.Id).ToList();

                foreach (var extraFile in extraFilesForTrackFile)
                {
                    movedFiles.AddIfNotNull(MoveFile(author, bookFile, extraFile));
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Author author, BookFile bookFile, string path, string extension, bool readOnly)
        {
            var extraFile = ImportFile(author, bookFile, path, readOnly, extension, null);

            _otherExtraFileService.Upsert(extraFile);

            return extraFile;
        }
    }
}
