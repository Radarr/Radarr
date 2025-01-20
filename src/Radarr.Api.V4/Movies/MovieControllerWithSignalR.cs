using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Radarr.Http.REST;

namespace Radarr.Api.V4.Movies
{
    public abstract class MovieControllerWithSignalR : RestControllerWithSignalR<MovieResource, Movie>,
                                                         IHandle<MovieGrabbedEvent>,
                                                         IHandle<MovieFileImportedEvent>,
                                                         IHandle<MovieFileDeletedEvent>
    {
        protected readonly IMovieService _movieService;
        protected readonly IMovieTranslationService _movieTranslationService;
        protected readonly IMovieStatisticsService _movieStatisticsService;
        protected readonly IConfigService _configService;
        protected readonly IMapCoversToLocal _coverMapper;

        protected MovieControllerWithSignalR(IMovieService movieService,
                                           IMovieTranslationService movieTranslationService,
                                           IMovieStatisticsService movieStatisticsService,
                                           IConfigService configService,
                                           IMapCoversToLocal coverMapper,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _movieTranslationService = movieTranslationService;
            _movieStatisticsService = movieStatisticsService;
            _configService = configService;
            _coverMapper = coverMapper;
        }

        protected MovieControllerWithSignalR(IMovieService movieService,
                                           IMovieTranslationService movieTranslationService,
                                           IMovieStatisticsService movieStatisticsService,
                                           IConfigService configService,
                                           IMapCoversToLocal coverMapper,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _movieTranslationService = movieTranslationService;
            _movieStatisticsService = movieStatisticsService;
            _configService = configService;
            _coverMapper = coverMapper;
        }

        protected override MovieResource GetResourceById(int id)
        {
            var movie = _movieService.GetMovie(id);

            return MapToResource(movie);
        }

        protected MovieResource MapToResource(Movie movie)
        {
            var availabilityDelay = _configService.AvailabilityDelay;
            var language = (Language)_configService.MovieInfoLanguage;

            var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);
            var translation = GetMovieTranslation(translations, movie.MovieMetadata, language);

            var resource = movie.ToResource(availabilityDelay, translation);
            FetchAndLinkMovieStatistics(resource);

            _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

            return resource;
        }

        protected List<MovieResource> MapToResource(List<Movie> movies)
        {
            var availabilityDelay = _configService.AvailabilityDelay;
            var movieLanguage = (Language)_configService.MovieInfoLanguage;

            var resources = new List<MovieResource>();

            foreach (var movie in movies)
            {
                var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);
                var translation = GetMovieTranslation(translations, movie.MovieMetadata, movieLanguage);

                var resource = movie.ToResource(availabilityDelay, translation);
                FetchAndLinkMovieStatistics(resource);

                resources.Add(resource);
            }

            return resources;
        }

        private static MovieTranslation? GetMovieTranslation(List<MovieTranslation> translations, MovieMetadata movieMetadata, Language translationLanguage)
        {
            if (translationLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movieMetadata.OriginalTitle,
                    Overview = movieMetadata.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == translationLanguage && t.MovieMetadataId == movieMetadata.Id);
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _movieStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
        }

        [NonAction]
        public void Handle(MovieGrabbedEvent message)
        {
            var resource = MapToResource(message.Movie.Movie);
            resource.Grabbed = true;

            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        [NonAction]
        public void Handle(MovieFileImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedMovie.Movie.Id);
        }

        [NonAction]
        public void Handle(MovieFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Movie.Id);
        }
    }
}
