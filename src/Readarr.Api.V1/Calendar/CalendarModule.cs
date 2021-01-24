using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Books;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Calendar
{
    public class CalendarModule : BookModuleWithSignalR
    {
        public CalendarModule(IBookService bookService,
                              ISeriesBookLinkService seriesBookLinkService,
                              IAuthorStatisticsService authorStatisticsService,
                              IMapCoversToLocal coverMapper,
                              IUpgradableSpecification upgradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
        : base(bookService, seriesBookLinkService, authorStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "calendar")
        {
            GetResourceAll = GetCalendar;
        }

        private List<BookResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");

            //TODO: Add Book Image support to BookModuleWithSignalR
            var includeBookImages = Request.GetBooleanQueryParameter("includeBookImages");

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

            var resources = MapToResource(_bookService.BooksBetweenDates(start, end, includeUnmonitored), includeAuthor);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
