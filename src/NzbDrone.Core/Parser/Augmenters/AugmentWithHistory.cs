using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithHistory : IAugmentParsedMovieInfo
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IEnumerable<IAugmentParsedMovieInfo> _augmenters;

        public AugmentWithHistory(IIndexerFactory indexerFactory, IEnumerable<IAugmentParsedMovieInfo> augmenters)
        {
            _indexerFactory = indexerFactory;
            _augmenters = augmenters;
        }

        public Type HelperType
        {
            get
            {
                return typeof(MovieHistory);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is MovieHistory history && history.EventType == MovieHistoryEventType.Grabbed)
            {
                //First we create a release info from history data.
                var releaseInfo = new ReleaseInfo();

                if (int.TryParse(history.Data.GetValueOrDefault("indexerId"), out var indexerId))
                {
                    //var indexerSettings = _indexerFactory.Get(indexerId).Settings as IIndexerSettings;
                    releaseInfo.IndexerId = indexerId;
                }

                if (long.TryParse(history.Data.GetValueOrDefault("size"), out var size))
                {
                    releaseInfo.Size = size;
                }

                if (Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags indexerFlags))
                {
                    releaseInfo.IndexerFlags = indexerFlags;
                }

                //Now we run the release info augmenters from the history release info. TODO: Add setting to only do that if you trust your indexer!
                var releaseInfoAugmenters = _augmenters.Where(a => a.HelperType.IsInstanceOfType(releaseInfo));
                foreach (var augmenter in releaseInfoAugmenters)
                {
                    movieInfo = augmenter.AugmentMovieInfo(movieInfo, releaseInfo);
                }
            }

            return movieInfo;
        }
    }
}
