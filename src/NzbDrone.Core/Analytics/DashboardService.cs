using System.Linq;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.AudiobookStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.BookStats;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MovieStats;

namespace NzbDrone.Core.Analytics
{
    public interface IDashboardService
    {
        DashboardStatistics GetStatistics();
    }

    public class DashboardService : IDashboardService
    {
        private readonly IMovieService _movieService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IBookService _bookService;
        private readonly IBookStatisticsService _bookStatisticsService;
        private readonly IAudiobookService _audiobookService;
        private readonly IAudiobookStatisticsService _audiobookStatisticsService;

        public DashboardService(IMovieService movieService,
                               IMovieStatisticsService movieStatisticsService,
                               IBookService bookService,
                               IBookStatisticsService bookStatisticsService,
                               IAudiobookService audiobookService,
                               IAudiobookStatisticsService audiobookStatisticsService)
        {
            _movieService = movieService;
            _movieStatisticsService = movieStatisticsService;
            _bookService = bookService;
            _bookStatisticsService = bookStatisticsService;
            _audiobookService = audiobookService;
            _audiobookStatisticsService = audiobookStatisticsService;
        }

        public DashboardStatistics GetStatistics()
        {
            var movieStats = GetMovieStatistics();
            var bookStats = GetBookStatistics();
            var audiobookStats = GetAudiobookStatistics();

            return new DashboardStatistics
            {
                Movies = movieStats,
                Books = bookStats,
                Audiobooks = audiobookStats,
                TotalSizeOnDisk = movieStats.SizeOnDisk + bookStats.SizeOnDisk + audiobookStats.SizeOnDisk
            };
        }

        private MediaTypeStatistics GetMovieStatistics()
        {
            var movies = _movieService.GetAllMovies();
            var stats = _movieStatisticsService.MovieStatistics();
            var statsDict = stats.ToDictionary(s => s.MovieId);

            var withFiles = 0;
            var missing = 0;
            var monitored = 0;
            var unmonitored = 0;
            long sizeOnDisk = 0;

            foreach (var movie in movies)
            {
                if (movie.Monitored)
                {
                    monitored++;
                }
                else
                {
                    unmonitored++;
                }

                if (statsDict.TryGetValue(movie.Id, out var stat))
                {
                    if (stat.MovieFileCount > 0)
                    {
                        withFiles++;
                    }
                    else if (movie.Monitored)
                    {
                        missing++;
                    }

                    sizeOnDisk += stat.SizeOnDisk;
                }
                else if (movie.Monitored)
                {
                    missing++;
                }
            }

            return new MediaTypeStatistics
            {
                Total = movies.Count,
                WithFiles = withFiles,
                Missing = missing,
                Monitored = monitored,
                Unmonitored = unmonitored,
                SizeOnDisk = sizeOnDisk
            };
        }

        private MediaTypeStatistics GetBookStatistics()
        {
            var books = _bookService.GetAllBooks();
            var stats = _bookStatisticsService.BookStatistics();
            var statsDict = stats.ToDictionary(s => s.BookId);

            var withFiles = 0;
            var missing = 0;
            var monitored = 0;
            var unmonitored = 0;
            long sizeOnDisk = 0;

            foreach (var book in books)
            {
                if (book.Monitored)
                {
                    monitored++;
                }
                else
                {
                    unmonitored++;
                }

                if (statsDict.TryGetValue(book.Id, out var stat))
                {
                    if (stat.BookFileCount > 0)
                    {
                        withFiles++;
                    }
                    else if (book.Monitored)
                    {
                        missing++;
                    }

                    sizeOnDisk += stat.SizeOnDisk;
                }
                else if (book.Monitored)
                {
                    missing++;
                }
            }

            return new MediaTypeStatistics
            {
                Total = books.Count,
                WithFiles = withFiles,
                Missing = missing,
                Monitored = monitored,
                Unmonitored = unmonitored,
                SizeOnDisk = sizeOnDisk
            };
        }

        private MediaTypeStatistics GetAudiobookStatistics()
        {
            var audiobooks = _audiobookService.GetAllAudiobooks();
            var stats = _audiobookStatisticsService.AudiobookStatistics();
            var statsDict = stats.ToDictionary(s => s.AudiobookId);

            var withFiles = 0;
            var missing = 0;
            var monitored = 0;
            var unmonitored = 0;
            long sizeOnDisk = 0;
            var totalDurationMinutes = 0;

            foreach (var audiobook in audiobooks)
            {
                if (audiobook.Monitored)
                {
                    monitored++;
                }
                else
                {
                    unmonitored++;
                }

                if (statsDict.TryGetValue(audiobook.Id, out var stat))
                {
                    if (stat.AudiobookFileCount > 0)
                    {
                        withFiles++;
                    }
                    else if (audiobook.Monitored)
                    {
                        missing++;
                    }

                    sizeOnDisk += stat.SizeOnDisk;
                    totalDurationMinutes += stat.TotalDurationMinutes;
                }
                else if (audiobook.Monitored)
                {
                    missing++;
                }
            }

            return new MediaTypeStatistics
            {
                Total = audiobooks.Count,
                WithFiles = withFiles,
                Missing = missing,
                Monitored = monitored,
                Unmonitored = unmonitored,
                SizeOnDisk = sizeOnDisk,
                TotalDurationMinutes = totalDurationMinutes
            };
        }
    }
}
