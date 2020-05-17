using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Movie GetMovie(int movieId);
        List<Movie> GetMovies(IEnumerable<int> movieIds);
        PagingSpec<Movie> Paged(PagingSpec<Movie> pagingSpec);
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        List<Movie> FindByTmdbId(List<int> tmdbids);
        Movie FindByTitle(string title);
        Movie FindByTitle(string title, int year);
        Movie FindByTitleInexact(string title, int? year);
        Movie FindByTitleSlug(string slug);
        Movie FindByPath(string path);
        List<string> AllMoviePaths();
        bool MovieExists(Movie movie);
        List<Movie> GetMoviesByFileId(int fileId);
        List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        void SetFileId(Movie movie, MovieFile movieFile);
        void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false);
        List<Movie> GetAllMovies();
        List<Movie> AllForTag(int tagId);
        Movie UpdateMovie(Movie movie);
        List<Movie> UpdateMovie(List<Movie> movie, bool useExistingRelativeFolder);
        List<Movie> FilterExistingMovies(List<Movie> movies);
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Movie movie);
    }

    public class MovieService : IMovieService, IHandle<MovieFileAddedEvent>,
                                               IHandle<MovieFileDeletedEvent>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IImportExclusionsService _exclusionService;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,
                             IEventAggregator eventAggregator,
                             IBuildFileNames fileNameBuilder,
                             IConfigService configService,
                             IImportExclusionsService exclusionService,
                             IBuildMoviePaths moviePathBuilder,
                             Logger logger)
        {
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
            _fileNameBuilder = fileNameBuilder;
            _configService = configService;
            _exclusionService = exclusionService;
            _moviePathBuilder = moviePathBuilder;
            _logger = logger;
        }

        public Movie GetMovie(int movieId)
        {
            return _movieRepository.Get(movieId);
        }

        public List<Movie> GetMovies(IEnumerable<int> movieIds)
        {
            return _movieRepository.Get(movieIds).ToList();
        }

        public PagingSpec<Movie> Paged(PagingSpec<Movie> pagingSpec)
        {
            return _movieRepository.GetPaged(pagingSpec);
        }

        public Movie AddMovie(Movie newMovie)
        {
            _movieRepository.Insert(newMovie);
            _eventAggregator.PublishEvent(new MovieAddedEvent(GetMovie(newMovie.Id)));

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies)
        {
            _movieRepository.InsertMany(newMovies);
            _eventAggregator.PublishEvent(new MoviesImportedEvent(newMovies.Select(s => s.Id).ToList()));

            return newMovies;
        }

        public Movie FindByTitle(string title)
        {
            return FindByTitle(title.CleanSeriesTitle(), null);
        }

        public Movie FindByTitle(string title, int year)
        {
            return FindByTitle(title.CleanSeriesTitle(), year as int?);
        }

        private Movie FindByTitle(string cleanTitle, int? year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();
            var cleanTitleWithRomanNumbers = cleanTitle;
            var cleanTitleWithArabicNumbers = cleanTitle;

            foreach (var arabicRomanNumeral in RomanNumeralParser.GetArabicRomanNumeralsMapping())
            {
                var arabicNumber = arabicRomanNumeral.ArabicNumeralAsString;
                var romanNumber = arabicRomanNumeral.RomanNumeral;
                cleanTitleWithRomanNumbers = cleanTitleWithRomanNumbers.Replace(arabicNumber, romanNumber);
                cleanTitleWithArabicNumbers = cleanTitleWithArabicNumbers.Replace(romanNumber, arabicNumber);
            }

            var candidates = _movieRepository.FindByTitles(new List<string> { cleanTitle, cleanTitleWithArabicNumbers, cleanTitleWithRomanNumbers });

            var result = candidates.Where(x => x.CleanTitle == cleanTitle).FirstWithYear(year);

            if (result == null)
            {
                result =
                    candidates.Where(movie => movie.CleanTitle == cleanTitleWithArabicNumbers).FirstWithYear(year) ??
                    candidates.Where(movie => movie.CleanTitle == cleanTitleWithRomanNumbers).FirstWithYear(year);

                if (result == null)
                {
                    result = candidates
                        .Where(m => m.AlternativeTitles.Any(t => t.CleanTitle == cleanTitle ||
                                                            t.CleanTitle == cleanTitleWithArabicNumbers ||
                                                            t.CleanTitle == cleanTitleWithRomanNumbers))
                        .FirstWithYear(year);
                }
            }

            return result;
        }

        public Movie FindByImdbId(string imdbid)
        {
            return _movieRepository.FindByImdbId(imdbid);
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return _movieRepository.FindByTmdbId(tmdbid);
        }

        public List<Movie> FindByTmdbId(List<int> tmdbids)
        {
            return _movieRepository.FindByTmdbId(tmdbids);
        }

        private List<Movie> FindByTitleInexactAll(string title)
        {
            // find any movie clean title within the provided release title
            string cleanTitle = title.CleanSeriesTitle();
            var list = _movieRepository.FindByTitleInexact(cleanTitle);
            if (!list.Any())
            {
                // no movie matched
                return list;
            }

            // build ordered list of movie by position in the search string
            var query =
                list.Select(movie => new
                {
                    position = cleanTitle.IndexOf(movie.CleanTitle),
                    length = movie.CleanTitle.Length,
                    movie = movie
                })
                    .Where(s => (s.position >= 0))
                    .ToList()
                    .OrderBy(s => s.position)
                    .ThenByDescending(s => s.length)
                    .Select(s => s.movie)
                    .ToList();

            return query;
        }

        public Movie FindByTitleInexact(string title)
        {
            var query = FindByTitleInexactAll(title);

            // get the leftmost movie that is the longest
            // movie are usually the first thing in release title, so we select the leftmost and longest match
            var match = query.First();

            _logger.Debug("Multiple movie matched {0} from title {1}", match.Title, title);
            foreach (var entry in query)
            {
                _logger.Debug("Multiple movie match candidate: {0} cleantitle: {1}", entry.Title, entry.CleanTitle);
            }

            return match;
        }

        public Movie FindByTitleInexact(string title, int? year)
        {
            return FindByTitleInexactAll(title).FirstWithYear(year);
        }

        public Movie FindByPath(string path)
        {
            return _movieRepository.FindByPath(path);
        }

        public List<string> AllMoviePaths()
        {
            return _movieRepository.AllMoviePaths();
        }

        public void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false)
        {
            var movie = _movieRepository.Get(movieId);
            if (addExclusion)
            {
                _exclusionService.AddExclusion(new ImportExclusion { TmdbId = movie.TmdbId, MovieTitle = movie.Title, MovieYear = movie.Year });
            }

            _movieRepository.Delete(movieId);
            _eventAggregator.PublishEvent(new MovieDeletedEvent(movie, deleteFiles));
            _logger.Info("Deleted movie {}", movie);
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public List<Movie> AllForTag(int tagId)
        {
            return GetAllMovies().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        public Movie UpdateMovie(Movie movie)
        {
            var storedMovie = GetMovie(movie.Id);

            var updatedMovie = _movieRepository.Update(movie);
            _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));

            return updatedMovie;
        }

        public List<Movie> UpdateMovie(List<Movie> movie, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} movie", movie.Count);
            foreach (var m in movie)
            {
                _logger.Trace("Updating: {0}", m.Title);

                if (!m.RootFolderPath.IsNullOrWhiteSpace())
                {
                    m.Path = _moviePathBuilder.BuildPath(m, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", m.Title, m.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", m.Title);
                }
            }

            _movieRepository.UpdateMany(movie);
            _logger.Debug("{0} movie updated", movie.Count);

            return movie;
        }

        public bool MoviePathExists(string folder)
        {
            return _movieRepository.MoviePathExists(folder);
        }

        public void RemoveAddOptions(Movie movie)
        {
            _movieRepository.SetFields(movie, s => s.AddOptions);
        }

        public void SetFileId(Movie movie, MovieFile movieFile)
        {
            _movieRepository.SetFileId(movieFile.Id, movie.Id);
            _logger.Info("Linking [{0}] > [{1}]", movieFile.RelativePath, movie);
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return _movieRepository.GetMoviesByFileId(fileId);
        }

        public Movie FindByTitleSlug(string slug)
        {
            return _movieRepository.FindByTitleSlug(slug);
        }

        public List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var movies = _movieRepository.MoviesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return movies;
        }

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            var movieResult = _movieRepository.MoviesWithoutFiles(pagingSpec);

            return movieResult;
        }

        public bool MovieExists(Movie movie)
        {
            Movie result = null;

            if (movie.TmdbId != 0)
            {
                result = _movieRepository.FindByTmdbId(movie.TmdbId);
                if (result != null)
                {
                    return true;
                }
            }

            if (movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                result = _movieRepository.FindByImdbId(movie.ImdbId);
                if (result != null)
                {
                    return true;
                }
            }

            if (movie.Title.IsNotNullOrWhiteSpace())
            {
                if (movie.Year > 1850)
                {
                    result = FindByTitle(movie.Title.CleanSeriesTitle(), movie.Year);
                    if (result != null)
                    {
                        return true;
                    }
                }
                else
                {
                    result = FindByTitle(movie.Title.CleanSeriesTitle());
                    if (result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<Movie> FilterExistingMovies(List<Movie> movies)
        {
            var allMovies = GetAllMovies();

            var withTmdbid = movies.Where(m => m.TmdbId != 0).ToList();
            var withoutTmdbid = movies.Where(m => m.TmdbId == 0).ToList();
            var withImdbid = withoutTmdbid.Where(m => m.ImdbId.IsNotNullOrWhiteSpace());
            var rest = withoutTmdbid.Where(m => m.ImdbId.IsNullOrWhiteSpace());

            var ret = withTmdbid.ExceptBy(m => m.TmdbId, allMovies, m => m.TmdbId, EqualityComparer<int>.Default)
                .Union(withImdbid.ExceptBy(m => m.ImdbId, allMovies, m => m.ImdbId, EqualityComparer<string>.Default))
                .Union(rest.ExceptBy(m => m.Title.CleanSeriesTitle(), allMovies, m => m.CleanTitle, EqualityComparer<string>.Default)).ToList();

            return ret;
        }

        public void Handle(MovieFileAddedEvent message)
        {
            var movie = message.MovieFile.Movie;
            movie.MovieFileId = message.MovieFile.Id;
            _movieRepository.Update(movie);

            //_movieRepository.SetFileId(message.MovieFile.Id, message.MovieFile.Movie.Value.Id);
            _logger.Info("Linking [{0}] > [{1}]", message.MovieFile.RelativePath, message.MovieFile.Movie);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            foreach (var movie in GetMoviesByFileId(message.MovieFile.Id))
            {
                _logger.Debug("Detaching movie {0} from file.", movie.Id);
                movie.MovieFileId = 0;

                if (message.Reason != DeleteMediaFileReason.Upgrade && _configService.AutoUnmonitorPreviouslyDownloadedMovies)
                {
                    movie.Monitored = false;
                }

                UpdateMovie(movie);
            }
        }
    }
}
