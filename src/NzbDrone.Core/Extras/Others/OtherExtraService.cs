using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IOtherExtraFileService _otherExtraFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IMediaFileAttributeService mediaFileAttributeService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _diskProvider = diskProvider;
            _otherExtraFileService = otherExtraFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Movie movie)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieFolder(Movie movie, string movieFolder)
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

        public override bool CanImportFile(LocalMovie localMovie, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            return true;
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalMovie localMovie, MovieFile movieFile, List<string> files, bool isReadOnly)
        {
            var importedFiles = new List<ExtraFile>();
            var filteredFiles = files.Where(f => CanImportFile(localMovie, movieFile, f, Path.GetExtension(f), isReadOnly)).ToList();
            var sourcePath = localMovie.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var matchingFiles = new List<string>();
            var hasNfo = false;

            foreach (var file in filteredFiles)
            {
                try
                {
                    // Filter out duplicate NFO files
                    if (file.EndsWith(".nfo", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (hasNfo)
                        {
                            continue;
                        }

                        hasNfo = true;
                    }

                    // Filename match
                    if (Path.GetFileNameWithoutExtension(file).StartsWith(sourceFileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingFiles.Add(file);
                        continue;
                    }

                    // Movie match
                    var fileMovieInfo = Parser.Parser.ParseMoviePath(file) ?? new ParsedMovieInfo();

                    if (fileMovieInfo.MovieTitle == null)
                    {
                        continue;
                    }

                    if (fileMovieInfo.MovieTitle == localMovie.FileMovieInfo.MovieTitle &&
                        fileMovieInfo.Year.Equals(localMovie.FileMovieInfo.Year))
                    {
                        matchingFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            foreach (string file in matchingFiles)
            {
                try
                {
                    var extraFile = ImportFile(localMovie.Movie, movieFile, file, isReadOnly, Path.GetExtension(file), null);
                    _mediaFileAttributeService.SetFilePermissions(file);
                    _otherExtraFileService.Upsert(extraFile);
                    importedFiles.Add(extraFile);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            return importedFiles;
        }
    }
}
