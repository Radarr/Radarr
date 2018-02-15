using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Movie;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Validation;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : MovieModule
    {
        public CalendarModule(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IMovieStatisticsService moviesStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper)
            : base(signalR, moviesService, moviesStatisticsService, sceneMappingService, coverMapper, "calendar")
        {
            
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

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);
            if (queryIncludeUnmonitored.HasValue) includeUnmonitored = Convert.ToBoolean(queryIncludeUnmonitored.Value);

            var resources = _moviesService.GetMoviesBetweenDates(start, end, includeUnmonitored).Select(MapToResource);

            return resources.OrderBy(e => e.InCinemas).ToList();
        }
    }
}
