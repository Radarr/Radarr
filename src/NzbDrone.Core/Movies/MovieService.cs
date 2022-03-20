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
        Movie FindByTitle(List<string> titles, int? year, List<string> otherTitles, List<Movie> candidates);
        List<Movie> FindByTitleCandidates(List<string> titles, out List<string> otherTitles);
        Movie FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        List<int> AllMovieTmdbIds();
        bool MovieExists(Movie movie);
        List<Movie> GetMoviesByFileId(int fileId);
        List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        void SetFileId(Movie movie, MovieFile movieFile);
        void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false);
        void DeleteMovies(List<int> movieIds, bool deleteFiles, bool addExclusion = false);
        List<Movie> GetAllMovies();
        Dictionary<int, List<int>> AllMovieTags();
        Movie UpdateMovie(Movie movie);
        List<Movie> UpdateMovie(List<Movie> movie, bool useExistingRelativeFolder);
        List<Movie> FilterExistingMovies(List<Movie> movies);
        List<int> GetRecommendedTmdbIds();
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Movie movie);
    }

    public class MovieService : IMovieService, IHandle<MovieFileAddedEvent>,
                                               IHandle<MovieFileDeletedEvent>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,
                            IEventAggregator eventAggregator,
                            IConfigService configService,
                            IBuildMoviePaths moviePathBuilder,
                            Logger logger)
        {
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
            _configService = configService;
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
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, null, otherTitles, candidates);
        }

        public Movie FindByTitle(string title, int year)
        {
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, year, otherTitles, candidates);
        }

        public Movie FindByTitle(List<string> cleanTitles, int? year, List<string> otherTitles, List<Movie> candidates)
        {
            var result = candidates.Where(x => cleanTitles.Contains(x.MovieMetadata.Value.CleanTitle)).FirstWithYear(year);

            if (result == null)
            {
                result =
                    candidates.Where(movie => otherTitles.Contains(movie.MovieMetadata.Value.CleanTitle)).FirstWithYear(year);
            }

            if (result == null)
            {
                result = candidates
                    .Where(m => m.MovieMetadata.Value.AlternativeTitles.Any(t => cleanTitles.Contains(t.CleanTitle) ||
                                                        otherTitles.Contains(t.CleanTitle)))
                    .FirstWithYear(year);
            }

            if (result == null)
            {
                result = candidates
                    .Where(m => m.MovieMetadata.Value.Translations.Any(t => cleanTitles.Contains(t.CleanTitle) ||
                                                        otherTitles.Contains(t.CleanTitle)))
                    .FirstWithYear(year);
            }

            return result;
        }

        public List<Movie> FindByTitleCandidates(List<string> titles, out List<string> otherTitles)
        {
            var lookupTitles = new List<string>();
            otherTitles = new List<string>();

            foreach (var title in titles)
            {
                var cleanTitle = title.CleanMovieTitle().ToLowerInvariant();
                var romanTitle = cleanTitle;
                var arabicTitle = cleanTitle;

                foreach (var arabicRomanNumeral in RomanNumeralParser.GetArabicRomanNumeralsMapping())
                {
                    var arabicNumber = arabicRomanNumeral.ArabicNumeralAsString;
                    var romanNumber = arabicRomanNumeral.RomanNumeral;

                    romanTitle = romanTitle.Replace(arabicNumber, romanNumber);
                    arabicTitle = arabicTitle.Replace(romanNumber, arabicNumber);
                }

                romanTitle = romanTitle.ToLowerInvariant();

                otherTitles.AddRange(new List<string> { arabicTitle, romanTitle });
                lookupTitles.AddRange(new List<string> { cleanTitle, arabicTitle, romanTitle });
            }

            return _movieRepository.FindByTitles(lookupTitles);
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

        public Movie FindByPath(string path)
        {
            return _movieRepository.FindByPath(path);
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            return _movieRepository.AllMoviePaths();
        }

        public List<int> AllMovieTmdbIds()
        {
            return _movieRepository.AllMovieTmdbIds();
        }

        public void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false)
        {
            var movie = _movieRepository.Get(movieId);

            _movieRepository.Delete(movieId);
            _eventAggregator.PublishEvent(new MoviesDeletedEvent(new List<Movie> { movie }, deleteFiles, addExclusion));
            _logger.Info("Deleted movie {0}", movie);
        }

        public void DeleteMovies(List<int> movieIds, bool deleteFiles, bool addExclusion = false)
        {
            var moviesToDelete = _movieRepository.Get(movieIds).ToList();

            _movieRepository.DeleteMany(movieIds);

            _eventAggregator.PublishEvent(new MoviesDeletedEvent(moviesToDelete, deleteFiles, addExclusion));

            foreach (var movie in moviesToDelete)
            {
                _logger.Info("Deleted movie {0}", movie);
            }
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            return _movieRepository.AllMovieTags();
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
            _logger.Info("Assigning file [{0}] to movie [{1}]", movieFile.RelativePath, movie);
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return _movieRepository.GetMoviesByFileId(fileId);
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
                    result = FindByTitle(movie.Title.CleanMovieTitle(), movie.Year);
                    if (result != null)
                    {
                        return true;
                    }
                }
                else
                {
                    result = FindByTitle(movie.Title.CleanMovieTitle());
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
                .Union(rest.ExceptBy(m => m.Title.CleanMovieTitle(), allMovies, m => m.MovieMetadata.Value.CleanTitle, EqualityComparer<string>.Default)).ToList();

            return ret;
        }

        public List<int> GetRecommendedTmdbIds()
        {
            return _movieRepository.GetRecommendations();
        }

        public void Handle(MovieFileAddedEvent message)
        {
            var movie = message.MovieFile.Movie;
            movie.MovieFileId = message.MovieFile.Id;
            _movieRepository.Update(movie);

            //_movieRepository.SetFileId(message.MovieFile.Id, message.MovieFile.Movie.Value.Id);
            _logger.Info("Assigning file [{0}] to movie [{1}]", message.MovieFile.RelativePath, message.MovieFile.Movie);
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
