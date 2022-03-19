using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
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
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IMovieTranslationService movieTranslationService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            IConfigService configService)
            : base(signalR)
        {
            _moviesService = moviesService;
            _movieTranslationService = movieTranslationService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
        }

        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public List<MovieResource> GetCalendar(
            DateTime? start,
            DateTime? end,
            bool unmonitored = false,
            string releaseTypes = "")
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);
            var filteredReleaseTypes = releaseTypes
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?.ToArray() ?? Array.Empty<string>();

            var resources = _moviesService.GetMoviesBetweenDates(startUse, endUse, unmonitored)
                .Select(movie => MapToResource(movie, filteredReleaseTypes));

            return resources.OrderBy(e => e.Title).ToList();
        }

        protected MovieResource MapToResource(Movie movie, string[] releaseTypes)
        {
            if (movie == null)
            {
                return null;
            }

            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.Id);
            var translation = GetMovieTranslation(translations, movie.MovieMetadata);
            var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);
            ApplyReleaseFilter(resource, releaseTypes);

            return resource;
        }

        private void ApplyReleaseFilter(MovieResource resource, string[] releaseTypes)
        {
            var meetsMinimumAvailability = releaseTypes.Contains(ReleaseType.MeetsMinimumAvailability);
            if (meetsMinimumAvailability && resource.MinimumAvailability == MovieStatusType.Released)
            {
                resource.InCinemas = null;
                return;
            }

            if (meetsMinimumAvailability || releaseTypes.Length == 0)
            {
                return;
            }

            if (!releaseTypes.Contains(ReleaseType.Digital))
            {
                resource.DigitalRelease = null;
            }

            if (!releaseTypes.Contains(ReleaseType.Physical))
            {
                resource.PhysicalRelease = null;
            }

            if (!releaseTypes.Contains(ReleaseType.InCinemas))
            {
                resource.InCinemas = null;
            }
        }

        private MovieTranslation GetMovieTranslation(List<MovieTranslation> translations, MovieMetadata movie)
        {
            if ((Language)_configService.MovieInfoLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage && t.MovieMetadataId == movie.Id);
        }
    }
}
