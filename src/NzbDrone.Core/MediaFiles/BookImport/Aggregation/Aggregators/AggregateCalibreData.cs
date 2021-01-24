using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Aggregation.Aggregators
{
    public class AggregateCalibreData : IAggregate<LocalBook>
    {
        private readonly Logger _logger;
        private readonly ICached<CalibreBook> _bookCache;

        public AggregateCalibreData(Logger logger,
                                    ICacheManager cacheManager)
        {
            _logger = logger;
            _bookCache = cacheManager.GetCache<CalibreBook>(typeof(CalibreProxy));

            _logger.Trace("Started calibre aug");
        }

        public LocalBook Aggregate(LocalBook localTrack, bool others)
        {
            var book = _bookCache.Find(localTrack.Path);
            _logger.Trace($"Searching calibre data for {localTrack.Path}");

            if (book != null)
            {
                _logger.Trace($"Using calibre data for {localTrack.Path}:\n{book.ToJson()}");

                var parsed = localTrack.FileTrackInfo;
                parsed.Asin = book.Identifiers.GetValueOrDefault("mobi-asin");
                parsed.Isbn = book.Identifiers.GetValueOrDefault("isbn");
                parsed.GoodreadsId = book.Identifiers.GetValueOrDefault("goodreads");
                parsed.AuthorTitle = book.AuthorSort;
                parsed.BookTitle = book.Title;
            }

            return localTrack;
        }
    }
}
