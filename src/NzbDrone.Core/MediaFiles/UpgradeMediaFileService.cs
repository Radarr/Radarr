using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        MovieFileMoveResult UpgradeMovieFile(MovieFile movieFile, LocalMovie localMovie, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveMovieFiles _movieFileMover;
    private readonly IRenameMovieFileService _movieFileRenamer;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMoveMovieFiles movieFileMover,
                                       IDiskProvider diskProvider,
                                       IRenameMovieFileService movieFileRenamer,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _movieFileMover = movieFileMover;
            _diskProvider = diskProvider;
            _movieFileRenamer = movieFileRenamer;
            _logger = logger;
        }

        public MovieFileMoveResult UpgradeMovieFile(MovieFile movieFile, LocalMovie localMovie, bool copyOnly = false)
        {
            _logger.Trace("Upgrading existing movie file.");
            var moveFileResult = new MovieFileMoveResult();

            var existingFile = localMovie.Movie.MovieFile;

            if (existingFile != null)
            {
                var movieFilePath = Path.Combine(localMovie.Movie.Path, existingFile.RelativePath);

                if (_diskProvider.FileExists(movieFilePath))
                {
                    _logger.Debug("Removing existing movie file: {0}", existingFile);
                    _recycleBinProvider.DeleteFile(movieFilePath);
                }

                moveFileResult.OldFiles.Add(existingFile);
                _mediaFileService.Delete(existingFile, DeleteMediaFileReason.Upgrade);
            }

        //Temporary for correctly getting path
        localMovie.Movie.MovieFileId = 1;
        localMovie.Movie.MovieFile = movieFile;

            if (copyOnly)
            {
                moveFileResult.MovieFile = _movieFileMover.CopyMovieFile(movieFile, localMovie);
            }
            else
            {
                moveFileResult.MovieFile = _movieFileMover.MoveMovieFile(movieFile, localMovie);
            }

            localMovie.Movie.MovieFileId = existingFile?.Id ?? 0;
            localMovie.Movie.MovieFile = existingFile;

        //_movieFileRenamer.RenameMoviePath(localMovie.Movie, false);

            return moveFileResult;
        }
    }
}
