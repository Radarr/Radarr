using System;
using System.Collections.Generic;
using NLog;

namespace NzbDrone.Core.MetadataSource.Book
{
    public class BookInfoProxy : IProvideBookInfo
    {
        private readonly Logger _logger;

        public BookInfoProxy(Logger logger)
        {
            _logger = logger;
        }

        public BookMetadata GetByExternalId(string externalId)
        {
            _logger.Debug("GetByExternalId called for: {0} (stub implementation)", externalId);
            return null;
        }

        public BookMetadata GetById(int providerId)
        {
            _logger.Debug("GetById called for: {0} (stub implementation)", providerId);
            return null;
        }

        public List<BookMetadata> GetBulkInfo(List<int> providerIds)
        {
            _logger.Debug("GetBulkInfo called for {0} IDs (stub implementation)", providerIds.Count);
            return new List<BookMetadata>();
        }

        public List<BookMetadata> GetTrending()
        {
            _logger.Debug("GetTrending called (stub implementation)");
            return new List<BookMetadata>();
        }

        public List<BookMetadata> GetPopular()
        {
            _logger.Debug("GetPopular called (stub implementation)");
            return new List<BookMetadata>();
        }

        public HashSet<int> GetChangedItems(DateTime startTime)
        {
            _logger.Debug("GetChangedItems called since {0} (stub implementation)", startTime);
            return new HashSet<int>();
        }

        public List<BookMetadata> SearchByTitle(string title)
        {
            _logger.Debug("SearchByTitle called for: {0} (stub implementation)", title);
            return new List<BookMetadata>();
        }

        public List<BookMetadata> SearchByTitle(string title, int year)
        {
            _logger.Debug("SearchByTitle called for: {0} ({1}) (stub implementation)", title, year);
            return new List<BookMetadata>();
        }

        public BookMetadata GetByIsbn(string isbn)
        {
            _logger.Debug("GetByIsbn called for: {0} (stub implementation)", isbn);
            return null;
        }

        public BookMetadata GetByIsbn13(string isbn13)
        {
            _logger.Debug("GetByIsbn13 called for: {0} (stub implementation)", isbn13);
            return null;
        }

        public BookMetadata GetByAsin(string asin)
        {
            _logger.Debug("GetByAsin called for: {0} (stub implementation)", asin);
            return null;
        }

        public List<BookMetadata> GetByAuthor(string authorName)
        {
            _logger.Debug("GetByAuthor called for: {0} (stub implementation)", authorName);
            return new List<BookMetadata>();
        }
    }
}
