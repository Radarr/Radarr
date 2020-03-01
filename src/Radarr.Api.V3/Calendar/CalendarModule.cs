using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Calendar
{
    public class CalendarModule : RadarrRestModuleWithSignalR<MovieResource, Movie>
    {
        private readonly IMovieService _moviesService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;

        public CalendarModule(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IUpgradableSpecification qualityUpgradableSpecification)
            : base(signalR, "calendar")
        {
            _moviesService = moviesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

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

            var resource = movie.ToResource(_qualityUpgradableSpecification);

            return resource;
        }
    }
}
