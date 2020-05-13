using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Albums;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Calendar
{
    public class CalendarModule : AlbumModuleWithSignalR
    {
        public CalendarModule(IBookService bookService,
                              IAuthorStatisticsService artistStatisticsService,
                              IMapCoversToLocal coverMapper,
                              IUpgradableSpecification upgradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
            : base(bookService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "calendar")
        {
            GetResourceAll = GetCalendar;
        }

        private List<AlbumResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");

            //TODO: Add Album Image support to AlbumModuleWithSignalR
            var includeAlbumImages = Request.GetBooleanQueryParameter("includeAlbumImages");

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue)
            {
                start = DateTime.Parse(queryStart.Value);
            }

            if (queryEnd.HasValue)
            {
                end = DateTime.Parse(queryEnd.Value);
            }

            var resources = MapToResource(_bookService.BooksBetweenDates(start, end, includeUnmonitored), includeArtist);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
