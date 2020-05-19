using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation
{
    public interface IRemoteBookAggregationService
    {
        RemoteBook Augment(RemoteBook remoteBook);
    }

    public class RemoteBookAggregationService : IRemoteBookAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteBook> _augmenters;
        private readonly Logger _logger;

        public RemoteBookAggregationService(IEnumerable<IAggregateRemoteBook> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteBook Augment(RemoteBook remoteBook)
        {
            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteBook);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteBook;
        }
    }
}
