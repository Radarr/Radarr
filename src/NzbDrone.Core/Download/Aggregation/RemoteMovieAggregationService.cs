using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation
{
    public interface IRemoteMovieAggregationService
    {
        RemoteMovie Augment(RemoteMovie remoteMovie);
    }

    public class RemoteMovieAggregationService : IRemoteMovieAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteMovie> _augmenters;
        private readonly Logger _logger;

        public RemoteMovieAggregationService(IEnumerable<IAggregateRemoteMovie> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteMovie Augment(RemoteMovie remoteMovie)
        {
            if (remoteMovie == null)
            {
                return null;
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteMovie);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteMovie;
        }
    }
}
