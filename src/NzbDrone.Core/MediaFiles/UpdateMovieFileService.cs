using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Exceptron;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpdateMovieFileService
    {
        void ChangeFileDateForFile(MovieFile movieFile, Movie movie);
    }

    public class UpdateMovieFileService : IUpdateMovieFileService,
                                            IHandle<MovieScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public UpdateMovieFileService(IDiskProvider diskProvider,
                                        IConfigService configService,
                                        IMovieService movieService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _movieService = movieService;
            _logger = logger;
        }

        public void ChangeFileDateForFile(MovieFile movieFile, Movie movie)
        {
            ChangeFileDate(movieFile, movie);
        }

        private bool ChangeFileDate(MovieFile movieFile, Movie movie)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

            switch (_configService.FileDate)
            {
                case FileDateType.Release:
                    {
                        var airDate = movie.PhysicalRelease;

                        if (airDate.HasValue == false)
                        {
                            return false;
                        }

                        return ChangeFileDate(movieFilePath, airDate.Value);
                    }

                case FileDateType.Cinemas:
                    {
                        var airDate = movie.InCinemas;

                        if (airDate.HasValue == false)
                        {
                            return false;
                        }

                        return ChangeFileDate(movieFilePath, airDate.Value);
                    }
            }

            return false;
        }

        private bool ChangeFileDate(string filePath, DateTime date)
        {
            DateTime oldDateTime = _diskProvider.FileGetLastWrite(filePath);

            if (!DateTime.Equals(date, oldDateTime))
            {
                try
                {
                    _diskProvider.FileSetLastWriteTime(filePath, date);
                    _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldDateTime, date);

                    return true;
                }

                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                }
            }

            return false;
        }

        public void Handle(MovieScannedEvent message)
        {
            if (_configService.FileDate == FileDateType.None)
            {
                return;
            }

            var movies = _movieService.MoviesWithFiles(message.Movie.Id);

            var movieFiles = new List<MovieFile>();
            var updated = new List<MovieFile>();

            foreach (var group in movies.GroupBy(e => e.MovieFileId))
            {
                var moviesInFile = group.Select(e => e).ToList();
                var movieFile = moviesInFile.First().MovieFile;

                movieFiles.Add(movieFile);

                if (ChangeFileDate(movieFile, message.Movie))
                {
                    updated.Add(movieFile);
                }
            }

            if (updated.Any())
            {
                _logger.ProgressDebug("Changed file date for {0} files of {1} in {2}", updated.Count, movieFiles.Count, message.Movie.Title);
            }

            else
            {
                _logger.ProgressDebug("No file dates changed for {0}", message.Movie.Title);
            }
        }

        private bool ChangeFileDateToLocalAirDate(string filePath, string fileDate, string fileTime)
        {
            DateTime airDate;

            if (DateTime.TryParse(fileDate + ' ' + fileTime, out airDate))
            {
                // avoiding false +ve checks and set date skewing by not using UTC (Windows)
                DateTime oldDateTime = _diskProvider.FileGetLastWrite(filePath);

                if (!DateTime.Equals(airDate, oldDateTime))
                {
                    try
                    {
                        _diskProvider.FileSetLastWriteTime(filePath, airDate);
                        _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldDateTime, airDate);

                        return true;
                    }

                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                    }
                }
            }

            else
            {
                _logger.Debug("Could not create valid date to change file [{0}]", filePath);
            }

            return false;
        }

        private bool ChangeFileDateToUtcAirDate(string filePath, DateTime airDateUtc)
        {
            DateTime oldLastWrite = _diskProvider.FileGetLastWrite(filePath);

            if (!DateTime.Equals(airDateUtc, oldLastWrite))
            {
                try
                {
                    _diskProvider.FileSetLastWriteTime(filePath, airDateUtc);
                    _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldLastWrite, airDateUtc);

                    return true;
                }

                catch (Exception ex)
                {
                    ex.ExceptronIgnoreOnMono();
                    _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                }
            }

            return false;
        }
    }
}
