using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.SignalR;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Calendar
{
    public class CalendarModule : RadarrRestModuleWithSignalR<MovieResource, Movie>
    {
        private readonly IMovieService _moviesService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;

        public CalendarModule(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IMovieTranslationService movieTranslationService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            IConfigService configService)
            : base(signalR, "calendar")
        {
            _moviesService = moviesService;
            _movieTranslationService = movieTranslationService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;

            GetResourceAll = GetCalendar;
        }

        private List<MovieResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = false;

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;
            var queryIncludeUnmonitored = Request.Query.Unmonitored;

            if (queryStart.HasValue)
            {
                start = DateTime.Parse(queryStart.Value);
            }

            if (queryEnd.HasValue)
            {
                end = DateTime.Parse(queryEnd.Value);
            }

            if (queryIncludeUnmonitored.HasValue)
            {
                includeUnmonitored = Convert.ToBoolean(queryIncludeUnmonitored.Value);
            }

            var resources = _moviesService.GetMoviesBetweenDates(start, end, includeUnmonitored).Select(MapToResource);

            return resources.OrderBy(e => e.InCinemas).ToList();
        }

        protected MovieResource MapToResource(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            var translation = _movieTranslationService.GetAllTranslationsForMovie(movie.Id).FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            var resource = movie.ToResource(_qualityUpgradableSpecification, translation);

            return resource;
        }
    }
}
