using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;
using Radarr.Api.V3.Movies;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Calendar
{
    [V3ApiController]
    public class CalendarController : RestControllerWithSignalR<MovieResource, Movie>
    {
        private readonly IMovieService _moviesService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly ITagService _tagService;
        private readonly IConfigService _configService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IMovieTranslationService movieTranslationService,
                            IMovieStatisticsService movieStatisticsService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            ITagService tagService,
                            IConfigService configService)
            : base(signalR)
        {
            _moviesService = moviesService;
            _movieTranslationService = movieTranslationService;
            _movieStatisticsService = movieStatisticsService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _tagService = tagService;
            _configService = configService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<MovieResource> GetCalendar(DateTime? start, DateTime? end, bool unmonitored = false, string tags = "")
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);
            var movies = _moviesService.GetMoviesBetweenDates(startUse, endUse, unmonitored);
            var parsedTags = new List<int>();
            var results = new List<Movie>();

            if (tags.IsNotNullOrWhiteSpace())
            {
                parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            foreach (var movie in movies)
            {
                if (movie == null)
                {
                    continue;
                }

                if (parsedTags.Any() && parsedTags.None(movie.Tags.Contains))
                {
                    continue;
                }

                results.Add(movie);
            }

            var resources = MapToResource(results);

            return resources.OrderBy(e => e.InCinemas).ToList();
        }

        protected List<MovieResource> MapToResource(List<Movie> movies)
        {
            var resources = new List<MovieResource>();
            var availDelay = _configService.AvailabilityDelay;
            var language = (Language)_configService.MovieInfoLanguage;

            foreach (var movie in movies)
            {
                if (movie == null)
                {
                    continue;
                }

                var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);
                var translation = GetMovieTranslation(translations, movie.MovieMetadata, language);

                var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);
                FetchAndLinkMovieStatistics(resource);

                resources.Add(resource);
            }

            return resources;
        }

        private MovieTranslation GetMovieTranslation(List<MovieTranslation> translations, MovieMetadata movie, Language language)
        {
            if (language == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == language && t.MovieMetadataId == movie.Id);
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _movieStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
            resource.HasFile = movieStatistics.MovieFileCount > 0;
            resource.SizeOnDisk = movieStatistics.SizeOnDisk;
        }
    }
}
