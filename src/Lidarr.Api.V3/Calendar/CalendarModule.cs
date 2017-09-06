using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Api.V3.Albums;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V3.Calendar
{
    public class CalendarModule : AlbumModuleWithSignalR
    {
        public CalendarModule(IAlbumService albumService,
                              IArtistStatisticsService artistStatisticsService,
                              IArtistService artistService,
                              IUpgradableSpecification ugradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, artistService, ugradableSpecification, signalRBroadcaster, "calendar")
        {
            GetResourceAll = GetCalendar;
        }

        private List<AlbumResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeEpisodeFile = Request.GetBooleanQueryParameter("includeEpisodeFile");

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var resources = MapToResource(_albumService.AlbumsBetweenDates(start, end, includeUnmonitored), includeArtist);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
