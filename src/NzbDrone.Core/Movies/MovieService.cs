using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.NetImport.ImportExclusions;

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
        Movie FindByTitle(string title);
        Movie FindByTitle(string title, int year);
        Movie FindByTitleInexact(string title, int? year);
        Movie FindByTitleSlug(string slug);
        bool MovieExists(Movie movie);
        Movie GetMovieByFileId(int fileId);
        List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
		void SetFileId(Movie movie, MovieFile movieFile);
        void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false);
        List<Movie> GetAllMovies();
        Movie UpdateMovie(Movie movie);
        List<Movie> UpdateMovie(List<Movie> movie);
        List<Movie> FilterExistingMovies(List<Movie> movies);
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Movie movie);
        List<Movie> MoviesWithFiles(int movieId);
        System.Linq.Expressions.Expression<Func<Movie, bool>> ConstructFilterExpression(string FilterKey, string FilterValue, string filterType = null);
    }

    public class MovieService : IMovieService, IHandle<MovieFileAddedEvent>,
                                               IHandle<MovieFileDeletedEvent>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IImportExclusionsService _exclusionService;
        private readonly Logger _logger;


        public MovieService(IMovieRepository movieRepository,
                             IEventAggregator eventAggregator,
                             IBuildFileNames fileNameBuilder,
                             IConfigService configService,
                             IImportExclusionsService exclusionService,
                             Logger logger)
        {
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
            _fileNameBuilder = fileNameBuilder;
            _configService = configService;
            _exclusionService = exclusionService;
            _logger = logger;
        }


        public System.Linq.Expressions.Expression<Func<Movie, bool>> ConstructFilterExpression(string FilterKey, string FilterValue, string FilterType = null)
        {
            //if (FilterKey == "all" && FilterValue == "all")
            //{
            //    return v => v.Monitored == true || v.Monitored == false;
            //}
            if (FilterKey == "monitored" && FilterValue == "false")
            {
                return v => v.Monitored == false;
            }
            else if (FilterKey == "monitored" && FilterValue == "true")
            {
                return v => v.Monitored == true;
            }
            else if (FilterKey == "status")
            {
                switch (FilterValue)
                {
                    case "released":
                        return v => v.Status == MovieStatusType.Released;
                        break;
                    case "inCinemas":
                        return v => v.Status == MovieStatusType.InCinemas;
                        break;
                    case "announced":
                        return v => v.Status == MovieStatusType.Announced;
                        break;
                    case "available":
                        return v => v.Monitored == true &&
                             ((v.MinimumAvailability == MovieStatusType.Released && v.Status >= MovieStatusType.Released) ||
                             (v.MinimumAvailability == MovieStatusType.InCinemas && v.Status >= MovieStatusType.InCinemas) ||
                             (v.MinimumAvailability == MovieStatusType.Announced && v.Status >= MovieStatusType.Announced) ||
                             (v.MinimumAvailability == MovieStatusType.PreDB && v.Status >= MovieStatusType.Released || v.HasPreDBEntry == true));
                        break;
                }
            }
            else if (FilterKey == "downloaded")
            {
                return v => v.MovieFileId == 0;
            }
            else if (FilterKey == "title")
            {
                if (FilterValue == string.Empty || FilterValue == null)
                {
                    return v => true;
                }
                else
                {
                    if (FilterType == "contains")
                    {
                        return v => v.CleanTitle.Contains(FilterValue);
                    }
                    else
                    {
                        return v => v.CleanTitle == FilterValue;
                    }
                }
            }
            return v => true;
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
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            MoviePathState defaultState = MoviePathState.Static;
            if (!_configService.PathsDefaultStatic)
            {
                defaultState = MoviePathState.Dynamic;
            }
            if (string.IsNullOrWhiteSpace(newMovie.Path))
            {
                var folderName = _fileNameBuilder.GetMovieFolder(newMovie);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
                newMovie.PathState = defaultState;
            }
            else
            {
                newMovie.PathState = defaultState == MoviePathState.Dynamic ? MoviePathState.StaticOnce : MoviePathState.Static;
            }

                _logger.Info("Adding Movie {0} Path: [{1}]", newMovie, newMovie.Path);

            newMovie.CleanTitle = newMovie.Title.CleanSeriesTitle();
            newMovie.SortTitle = MovieTitleNormalizer.Normalize(newMovie.Title, newMovie.TmdbId);
            newMovie.Added = DateTime.UtcNow;

            _movieRepository.Insert(newMovie);
            _eventAggregator.PublishEvent(new MovieAddedEvent(GetMovie(newMovie.Id)));

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies)
        {
            newMovies.ForEach(m => Ensure.That(m, () => m).IsNotNull());

            newMovies.ForEach(m =>
            {
                 MoviePathState defaultState = MoviePathState.Static;
	            if (!_configService.PathsDefaultStatic)
	            {
	                defaultState = MoviePathState.Dynamic;
	            }
                if (string.IsNullOrWhiteSpace(m.Path))
                {
                    var folderName = _fileNameBuilder.GetMovieFolder(m);
                    m.Path = Path.Combine(m.RootFolderPath, folderName);
                    m.PathState = defaultState;
                }
                else
                {
                    m.PathState = defaultState == MoviePathState.Dynamic ? MoviePathState.StaticOnce : MoviePathState.Static;
                }

                m.CleanTitle = m.Title.CleanSeriesTitle();
                m.SortTitle = MovieTitleNormalizer.Normalize(m.Title, m.TmdbId);
                m.Added = DateTime.UtcNow;
            });

            var existingMovies = GetAllMovies();
            var potentialMovieCount = newMovies.Count;

            newMovies = newMovies.DistinctBy(movie => movie.TmdbId).ToList(); // Ensure we don't add the same movie twice

            newMovies = newMovies.ExceptBy(n => n.TmdbId, existingMovies, e => e.TmdbId, EqualityComparer<int>.Default).ToList(); // Ensure we don't add a movie that already exists

            _movieRepository.InsertMany(newMovies);

            _logger.Debug("Adding {0} movies, {1} duplicates detected and skipped", newMovies.Count, potentialMovieCount - newMovies.Count);

            _eventAggregator.PublishEvent(new MoviesImportedEvent(newMovies.Select(s => s.Id).ToList()));

            return newMovies;
        }

        public Movie FindByTitle(string title)
        {
            return _movieRepository.FindByTitle(title.CleanSeriesTitle());
        }

        public Movie FindByImdbId(string imdbid)
        {
            return _movieRepository.FindByImdbId(imdbid);
        }

        private List<Movie> FindByTitleInexactAll(string title)
        {
            // find any movie clean title within the provided release title
            string cleanTitle = title.CleanSeriesTitle();
            var list = _movieRepository.All().Where(s => cleanTitle.Contains(s.CleanTitle))
                .Union(_movieRepository.All().Where(s => s.CleanTitle.Contains(cleanTitle))).ToList();
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
                    .Where(s => (s.position>=0))
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

        public Movie FindByTitle(string title, int year)
        {
            return _movieRepository.FindByTitle(title.CleanSeriesTitle(), year);
        }

        public void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false)
        {
            var movie = _movieRepository.Get(movieId);
            if (addExclusion)
            {
                _exclusionService.AddExclusion(new ImportExclusion {TmdbId = movie.TmdbId, MovieTitle = movie.Title, MovieYear = movie.Year } );
            }
            _movieRepository.Delete(movieId);
            _eventAggregator.PublishEvent(new MovieDeletedEvent(movie, deleteFiles));
            _logger.Info("Deleted movie {}", movie);
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Movie UpdateMovie(Movie movie)
        {
            var storedMovie = GetMovie(movie.Id);

            var updatedMovie = _movieRepository.Update(movie);
            _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));

            return updatedMovie;
        }

        public List<Movie> UpdateMovie(List<Movie> movie)
        {
            _logger.Debug("Updating {0} movie", movie.Count);
            foreach (var s in movie)
            {
                _logger.Trace("Updating: {0}", s.Title);
                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    var folderName = new DirectoryInfo(s.Path).Name;
                    s.Path = Path.Combine(s.RootFolderPath, folderName);
                    _logger.Trace("Changing path for {0} to {1}", s.Title, s.Path);
                }

                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Title);
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

        public void Handle(MovieFileAddedEvent message)
        {
            var movie = message.MovieFile.Movie.Value;
            movie.MovieFileId = message.MovieFile.Id;
            _movieRepository.Update(movie);
            //_movieRepository.SetFileId(message.MovieFile.Id, message.MovieFile.Movie.Value.Id);
            _logger.Info("Linking [{0}] > [{1}]", message.MovieFile.RelativePath, message.MovieFile.Movie.Value);
        }

		public void SetFileId(Movie movie, MovieFile movieFile)
		{
			_movieRepository.SetFileId(movieFile.Id, movie.Id);
			_logger.Info("Linking [{0}] > [{1}]", movieFile.RelativePath, movie);
		}

        public void Handle(MovieFileDeletedEvent message)
        {

            var movie = _movieRepository.GetMoviesByFileId(message.MovieFile.Id).First();
            movie.MovieFileId = 0;
            _logger.Debug("Detaching movie {0} from file.", movie.Id);

            if (message.Reason != DeleteMediaFileReason.Upgrade && _configService.AutoUnmonitorPreviouslyDownloadedMovies)
            {
                movie.Monitored = false;
            }

            UpdateMovie(movie);
        }

        public Movie GetMovieByFileId(int fileId)
        {
            return _movieRepository.GetMoviesByFileId(fileId).First();
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

        public List<Movie> MoviesWithFiles(int movieId)
        {
            return _movieRepository.MoviesWithFiles(movieId);
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

            if (movie.Year > 1850)
            {
                result = _movieRepository.FindByTitle(movie.Title.CleanSeriesTitle(), movie.Year);
                if (result != null)
                {
                    return true;
                }
            }
            else
            {
                result = _movieRepository.FindByTitle(movie.Title.CleanSeriesTitle());
                if (result != null)
                {
                    return true;
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
    }
}
