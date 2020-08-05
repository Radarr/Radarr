using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IOtherExtraFileService _otherExtraFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IMediaFileAttributeService mediaFileAttributeService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _otherExtraFileService = otherExtraFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, string movieFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles)
        {
            var extraFiles = _otherExtraFileService.GetFilesByMovie(movie.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var movieFile in movieFiles)
            {
                var extraFilesForMovieFile = extraFiles.Where(m => m.MovieFileId == movieFile.Id).ToList();

                foreach (var extraFile in extraFilesForMovieFile)
                {
                    movedFiles.AddIfNotNull(MoveFile(movie, movieFile, extraFile));
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Movie movie, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            var extraFile = ImportFile(movie, movieFile, path, readOnly, extension, null);

            if (extraFile != null)
            {
                _mediaFileAttributeService.SetFilePermissions(path);
                _otherExtraFileService.Upsert(extraFile);
            }

            return extraFile;
        }
    }
}
