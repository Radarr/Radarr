using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        EpisodeFileMoveResult UpgradeEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode, bool copyOnly = false);
        MovieFileMoveResult UpgradeMovieFile(MovieFile movieFile, LocalMovie localMovie, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IMoveMovieFiles _movieFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMoveEpisodeFiles episodeFileMover,
                                       IMoveMovieFiles movieFileMover,
                                       IDiskProvider diskProvider,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _movieFileMover = movieFileMover;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public MovieFileMoveResult UpgradeMovieFile(MovieFile episodeFile, LocalMovie localEpisode, bool copyOnly = false)
        {
            _logger.Trace("Upgrading existing episode file.");
            var moveFileResult = new MovieFileMoveResult();
            localEpisode.Movie.MovieFile.LazyLoad();
            var existingFile = localEpisode.Movie.MovieFile;
            existingFile.LazyLoad();

            if (existingFile.IsLoaded)
            {
                var file = existingFile.Value;
                var episodeFilePath = Path.Combine(localEpisode.Movie.Path, file.RelativePath);

                if (_diskProvider.FileExists(episodeFilePath))
                {
                    _logger.Debug("Removing existing episode file: {0}", file);
                    _recycleBinProvider.DeleteFile(episodeFilePath);
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }
            else
            {
                _logger.Warn("The existing movie file was not lazy loaded.");
            }

            

            if (copyOnly)
            {
                moveFileResult.MovieFile = _movieFileMover.CopyMovieFile(episodeFile, localEpisode);
            }
            else
            {
                moveFileResult.MovieFile= _movieFileMover.MoveMovieFile(episodeFile, localEpisode);
            }

            return moveFileResult;
        }

        public EpisodeFileMoveResult UpgradeEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode, bool copyOnly = false)
        {
            var moveFileResult = new EpisodeFileMoveResult();
            var existingFiles = localEpisode.Episodes
                                            .Where(e => e.EpisodeFileId > 0)
                                            .Select(e => e.EpisodeFile.Value)
                                            .GroupBy(e => e.Id);

            foreach (var existingFile in existingFiles)
            {
                var file = existingFile.First();
                var episodeFilePath = Path.Combine(localEpisode.Series.Path, file.RelativePath);

                if (_diskProvider.FileExists(episodeFilePath))
                {
                    _logger.Debug("Removing existing episode file: {0}", file);
                    _recycleBinProvider.DeleteFile(episodeFilePath);
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            if (copyOnly)
            {
                moveFileResult.EpisodeFile = _episodeFileMover.CopyEpisodeFile(episodeFile, localEpisode);
            }
            else
            {
                moveFileResult.EpisodeFile = _episodeFileMover.MoveEpisodeFile(episodeFile, localEpisode);
            }

            return moveFileResult;
        }
    }
}
