using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Books
{
    public interface IRefreshSeriesService
    {
        bool RefreshSeriesInfo(int authorMetadataId, List<Series> remoteBooks, Author remoteData, bool forceBookRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);
    }

    public class RefreshSeriesService : RefreshEntityServiceBase<Series, SeriesBookLink>, IRefreshSeriesService
    {
        private readonly IBookService _bookService;
        private readonly ISeriesService _seriesService;
        private readonly ISeriesBookLinkService _linkService;
        private readonly IRefreshSeriesBookLinkService _refreshLinkService;
        private readonly Logger _logger;

        public RefreshSeriesService(IBookService bookService,
                                    ISeriesService seriesService,
                                    ISeriesBookLinkService linkService,
                                    IRefreshSeriesBookLinkService refreshLinkService,
                                    IAuthorMetadataService authorMetadataService,
                                    Logger logger)
        : base(logger, authorMetadataService)
        {
            _bookService = bookService;
            _seriesService = seriesService;
            _linkService = linkService;
            _refreshLinkService = refreshLinkService;
            _logger = logger;
        }

        protected override RemoteData GetRemoteData(Series local, List<Series> remote, Author data)
        {
            return new RemoteData
            {
                Entity = remote.SingleOrDefault(x => x.ForeignSeriesId == local.ForeignSeriesId)
            };
        }

        protected override bool IsMerge(Series local, Series remote)
        {
            return local.ForeignSeriesId != remote.ForeignSeriesId;
        }

        protected override UpdateResult UpdateEntity(Series local, Series remote)
        {
            if (local.Equals(remote))
            {
                return UpdateResult.None;
            }

            local.UseMetadataFrom(remote);

            return UpdateResult.UpdateTags;
        }

        protected override Series GetEntityByForeignId(Series local)
        {
            return _seriesService.FindById(local.ForeignSeriesId);
        }

        protected override void SaveEntity(Series local)
        {
            // Use UpdateMany to avoid firing the book edited event
            _seriesService.UpdateMany(new List<Series> { local });
        }

        protected override void DeleteEntity(Series local, bool deleteFiles)
        {
            _seriesService.Delete(local.Id);
        }

        protected override List<SeriesBookLink> GetRemoteChildren(Series local, Series remote)
        {
            return remote.LinkItems;
        }

        protected override List<SeriesBookLink> GetLocalChildren(Series entity, List<SeriesBookLink> remoteChildren)
        {
            return _linkService.GetLinksBySeries(entity.Id);
        }

        protected override Tuple<SeriesBookLink, List<SeriesBookLink>> GetMatchingExistingChildren(List<SeriesBookLink> existingChildren, SeriesBookLink remote)
        {
            var existingChild = existingChildren.SingleOrDefault(x => x.BookId == remote.Book.Value.Id);
            var mergeChildren = new List<SeriesBookLink>();
            return Tuple.Create(existingChild, mergeChildren);
        }

        protected override void PrepareNewChild(SeriesBookLink child, Series entity)
        {
            child.Series = entity;
            child.SeriesId = entity.Id;
            child.BookId = child.Book.Value.Id;
        }

        protected override void PrepareExistingChild(SeriesBookLink local, SeriesBookLink remote, Series entity)
        {
            local.Series = entity;
            local.SeriesId = entity.Id;

            remote.Id = local.Id;
            remote.BookId = local.BookId;
            remote.SeriesId = entity.Id;
        }

        protected override void AddChildren(List<SeriesBookLink> children)
        {
            _linkService.InsertMany(children);
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<SeriesBookLink> remoteChildren, Author remoteData, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            return _refreshLinkService.RefreshSeriesBookLinkInfo(localChildren.Added, localChildren.Updated, localChildren.Merged, localChildren.Deleted, localChildren.UpToDate, remoteChildren, forceUpdateFileTags);
        }

        public bool RefreshSeriesInfo(int authorMetadataId, List<Series> remoteSeries, Author remoteData, bool forceBookRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            var updated = false;

            var existingByAuthor = _seriesService.GetByAuthorMetadataId(authorMetadataId);
            var existingBySeries = _seriesService.FindById(remoteSeries.Select(x => x.ForeignSeriesId));
            var existing = existingByAuthor.Concat(existingBySeries).GroupBy(x => x.ForeignSeriesId).Select(x => x.First()).ToList();

            var books = _bookService.GetBooksByAuthorMetadataId(authorMetadataId);
            var bookDict = books.ToDictionary(x => x.ForeignBookId);
            var links = new List<SeriesBookLink>();

            foreach (var s in remoteData.Series.Value)
            {
                s.LinkItems.Value.ForEach(x => x.Series = s);
                links.AddRange(s.LinkItems.Value.Where(x => bookDict.ContainsKey(x.Book.Value.ForeignBookId)));
            }

            var grouped = links.GroupBy(x => x.Series.Value);

            // Put in the links that go with the books we actually have
            foreach (var group in grouped)
            {
                group.Key.LinkItems = group.ToList();
            }

            remoteSeries = grouped.Select(x => x.Key).ToList();

            var toAdd = remoteSeries.ExceptBy(x => x.ForeignSeriesId, existing, x => x.ForeignSeriesId, StringComparer.Ordinal).ToList();
            var all = toAdd.Union(existing).ToList();

            _seriesService.InsertMany(toAdd);

            foreach (var item in all)
            {
                updated |= RefreshEntityInfo(item, remoteSeries, remoteData, true, forceUpdateFileTags, null);
            }

            return updated;
        }
    }
}
