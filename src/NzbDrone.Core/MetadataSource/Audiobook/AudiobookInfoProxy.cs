using System;
using System.Collections.Generic;
using NLog;

namespace NzbDrone.Core.MetadataSource.Audiobook
{
    public class AudiobookInfoProxy : IProvideAudiobookInfo
    {
        private readonly Logger _logger;

        public AudiobookInfoProxy(Logger logger)
        {
            _logger = logger;
        }

        public AudiobookMetadata GetByExternalId(string externalId)
        {
            _logger.Debug("GetByExternalId called for: {0} (stub implementation)", externalId);
            return null;
        }

        public AudiobookMetadata GetById(int providerId)
        {
            _logger.Debug("GetById called for: {0} (stub implementation)", providerId);
            return null;
        }

        public List<AudiobookMetadata> GetBulkInfo(List<int> providerIds)
        {
            _logger.Debug("GetBulkInfo called for {0} IDs (stub implementation)", providerIds.Count);
            return new List<AudiobookMetadata>();
        }

        public List<AudiobookMetadata> GetTrending()
        {
            _logger.Debug("GetTrending called (stub implementation)");
            return new List<AudiobookMetadata>();
        }

        public List<AudiobookMetadata> GetPopular()
        {
            _logger.Debug("GetPopular called (stub implementation)");
            return new List<AudiobookMetadata>();
        }

        public HashSet<int> GetChangedItems(DateTime startTime)
        {
            _logger.Debug("GetChangedItems called since {0} (stub implementation)", startTime);
            return new HashSet<int>();
        }

        public List<AudiobookMetadata> SearchByTitle(string title)
        {
            _logger.Debug("SearchByTitle called for: {0} (stub implementation)", title);
            return new List<AudiobookMetadata>();
        }

        public List<AudiobookMetadata> SearchByTitle(string title, int year)
        {
            _logger.Debug("SearchByTitle called for: {0} ({1}) (stub implementation)", title, year);
            return new List<AudiobookMetadata>();
        }

        public AudiobookMetadata GetByIsbn(string isbn)
        {
            _logger.Debug("GetByIsbn called for: {0} (stub implementation)", isbn);
            return null;
        }

        public AudiobookMetadata GetByAsin(string asin)
        {
            _logger.Debug("GetByAsin called for: {0} (stub implementation)", asin);
            return null;
        }

        public List<AudiobookMetadata> GetByNarrator(string narratorName)
        {
            _logger.Debug("GetByNarrator called for: {0} (stub implementation)", narratorName);
            return new List<AudiobookMetadata>();
        }

        public List<AudiobookMetadata> GetByAuthor(string authorName)
        {
            _logger.Debug("GetByAuthor called for: {0} (stub implementation)", authorName);
            return new List<AudiobookMetadata>();
        }
    }
}
