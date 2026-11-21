using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Movie movie, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IMovieVersionService _movieVersionService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IMovieVersionService movieVersionService,
                                            IMovieService movieService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _movieVersionService = movieVersionService;
            _movieService = movieService;
            _logger = logger;
        }

        public void Clean(Movie movie, List<string> filesOnDisk)
        {
            var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id).ForVersionOf(movie, _movieVersionService.GetAdditionalVersionIds(movie.Id));

            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);

            foreach (var movieFile in movieFiles)
            {
                var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(movieFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", movieFilePath);
                        _mediaFileService.Delete(movieFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Unable to cleanup MovieFile in DB: {0}", movieFile.Id);
                    _logger.Error(ex, errorMessage);
                }
            }
        }
    }
}
