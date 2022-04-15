using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.MovieStats
{
    public interface IMovieStatisticsService
    {
        List<MovieStatistics> MovieStatistics();
        MovieStatistics MovieStatistics(int movieId);
    }

    public class SeriesStatisticsService : IMovieStatisticsService
    {
        private readonly IMediaFileService _movieFileService;

        public SeriesStatisticsService(IMediaFileService movieFileService)
        {
            _movieFileService = movieFileService;
        }

        public List<MovieStatistics> MovieStatistics()
        {
            var seasonStatistics = _movieFileService.GetAllMediaFiles();

            return seasonStatistics.GroupBy(s => s.MovieId).Select(s => MapSeriesStatistics(s.ToList())).ToList();
        }

        public MovieStatistics MovieStatistics(int movieId)
        {
            var stats = _movieFileService.GetFilesByMovie(movieId);

            if (stats == null || stats.Count == 0)
            {
                return new MovieStatistics();
            }

            return MapSeriesStatistics(stats);
        }

        private MovieStatistics MapSeriesStatistics(List<MovieFile> movieFiles)
        {
            var seriesStatistics = new MovieStatistics
            {
                MovieId = movieFiles.First().MovieId,
                MovieFileCount = movieFiles.Count(),
                SizeOnDisk = movieFiles.Sum(s => s.Size),
                ReleaseGroups = movieFiles.Select(s => s.ReleaseGroup).Distinct().ToList()
            };

            return seriesStatistics;
        }
    }
}
