using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.MovieStats
{
    public interface IMovieStatisticsService
    {
        List<MovieStatistics> MovieStatistics();
        MovieStatistics MovieStatistics(int movieId);
    }

    public class MovieStatisticsService : IMovieStatisticsService
    {
        private readonly IMovieStatisticsRepository _movieStatisticsRepository;

        public MovieStatisticsService(IMovieStatisticsRepository movieStatisticsRepository)
        {
            _movieStatisticsRepository = movieStatisticsRepository;
        }

        public List<MovieStatistics> MovieStatistics()
        {
            var seasonStatistics = _movieStatisticsRepository.MovieStatistics();

            return seasonStatistics.GroupBy(s => s.MovieId).Select(s => MapMovieStatistics(s.ToList())).ToList();
        }

        public MovieStatistics MovieStatistics(int movieId)
        {
            var stats = _movieStatisticsRepository.MovieStatistics(movieId);

            if (stats == null || stats.Count == 0) return new MovieStatistics();

            return MapMovieStatistics(stats);
        }

        private MovieStatistics MapMovieStatistics(List<SeasonStatistics> seasonStatistics)
        {
            var movieStatistics = new MovieStatistics
                                   {
                                       SeasonStatistics = seasonStatistics,
                                       MovieId = seasonStatistics.First().MovieId,
                                       EpisodeFileCount = seasonStatistics.Sum(s => s.EpisodeFileCount),
                                       EpisodeCount = seasonStatistics.Sum(s => s.EpisodeCount),
                                       TotalEpisodeCount = seasonStatistics.Sum(s => s.TotalEpisodeCount),
                                       SizeOnDisk = seasonStatistics.Sum(s => s.SizeOnDisk)
                                   };

            var nextAiring = seasonStatistics.Where(s => s.NextAiring != null)
                                             .OrderBy(s => s.NextAiring)
                                             .FirstOrDefault();

            var previousAiring = seasonStatistics.Where(s => s.PreviousAiring != null)
                                                 .OrderBy(s => s.PreviousAiring)
                                                 .LastOrDefault();

            movieStatistics.NextAiringString = nextAiring != null ? nextAiring.NextAiringString : null;
            movieStatistics.PreviousAiringString = previousAiring != null ? previousAiring.PreviousAiringString : null;

            return movieStatistics;
        }
    }
}
