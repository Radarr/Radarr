using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation
{
    public interface IRemoteAlbumAggregationService
    {
        RemoteBook Augment(RemoteBook remoteAlbum);
    }

    public class RemoteAlbumAggregationService : IRemoteAlbumAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteAlbum> _augmenters;
        private readonly Logger _logger;

        public RemoteAlbumAggregationService(IEnumerable<IAggregateRemoteAlbum> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteBook Augment(RemoteBook remoteAlbum)
        {
            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteAlbum);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteAlbum;
        }
    }
}
