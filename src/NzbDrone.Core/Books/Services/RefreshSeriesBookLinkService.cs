using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Books
{
    public interface IRefreshSeriesBookLinkService
    {
        bool RefreshSeriesBookLinkInfo(List<SeriesBookLink> add, List<SeriesBookLink> update, List<Tuple<SeriesBookLink, SeriesBookLink>> merge, List<SeriesBookLink> delete, List<SeriesBookLink> upToDate, List<SeriesBookLink> remoteSeriesBookLinks, bool forceUpdateFileTags);
    }

    public class RefreshSeriesBookLinkService : IRefreshSeriesBookLinkService
    {
        private readonly ISeriesBookLinkService _seriesBookLinkService;
        private readonly Logger _logger;

        public RefreshSeriesBookLinkService(ISeriesBookLinkService trackService,
                                            Logger logger)
        {
            _seriesBookLinkService = trackService;
            _logger = logger;
        }

        public bool RefreshSeriesBookLinkInfo(List<SeriesBookLink> add, List<SeriesBookLink> update, List<Tuple<SeriesBookLink, SeriesBookLink>> merge, List<SeriesBookLink> delete, List<SeriesBookLink> upToDate, List<SeriesBookLink> remoteSeriesBookLinks, bool forceUpdateFileTags)
        {
            var updateList = new List<SeriesBookLink>();

            foreach (var link in update)
            {
                var remoteSeriesBookLink = remoteSeriesBookLinks.Single(e => e.Book.Value.Id == link.BookId);
                link.UseMetadataFrom(remoteSeriesBookLink);

                // make sure title is not null
                updateList.Add(link);
            }

            _seriesBookLinkService.DeleteMany(delete);
            _seriesBookLinkService.UpdateMany(updateList);

            return add.Any() || delete.Any() || updateList.Any() || merge.Any();
        }
    }
}
