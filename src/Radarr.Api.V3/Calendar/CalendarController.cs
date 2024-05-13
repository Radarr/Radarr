using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Calendar
{
    [V3ApiController]
    public class CalendarController : MovieControllerWithSignalR
    {
        private readonly IMovieService _moviesService;
        private readonly ITagService _tagService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IMovieService movieService,
                            IMovieTranslationService movieTranslationService,
                            IMovieStatisticsService movieStatisticsService,
                            IUpgradableSpecification upgradableSpecification,
                            ICustomFormatCalculationService formatCalculator,
                            ITagService tagService,
                            IConfigService configService)
            : base(movieService, movieTranslationService, movieStatisticsService, upgradableSpecification, formatCalculator, configService, signalR)
        {
            _moviesService = movieService;
            _tagService = tagService;
        }

        [NonAction]
        public override ActionResult<MovieResource> GetResourceByIdWithErrorHandler(int id)
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
    }
}
