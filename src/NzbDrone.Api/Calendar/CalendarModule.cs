using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Albums;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : AlbumModuleWithSignalR
    {
        public CalendarModule(IAlbumService albumService,
                              IArtistStatisticsService artistStatisticsService,
                              IArtistService artistService,
                              IQualityUpgradableSpecification qualityUpgradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, artistService, qualityUpgradableSpecification, signalRBroadcaster, "calendar")
        {
            GetResourceAll = GetCalendar;
        }

        private List<AlbumResource> GetCalendar()
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

            var resources = MapToResource(_albumService.AlbumsBetweenDates(start, end, includeUnmonitored), true);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
