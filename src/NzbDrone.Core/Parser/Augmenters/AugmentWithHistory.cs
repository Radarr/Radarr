using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

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
                return typeof(History.History);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is History.History history && history.EventType == HistoryEventType.Grabbed)
            {
                //First we create a release info from history data.
                var releaseInfo = new ReleaseInfo();

                if (int.TryParse(history.Data.GetValueOrDefault("IndexerId"), out var indexerId))
                {
                    var indexerSettings = _indexerFactory.Get(indexerId).Settings as IIndexerSettings;
                    releaseInfo.IndexerSettings = indexerSettings;
                }

                if (int.TryParse(history.Data.GetValueOrDefault("Size"), out var size))
                {
                    releaseInfo.Size = size;
                }

                if (Enum.TryParse(history.Data.GetValueOrDefault("IndexerFlags"), out IndexerFlags indexerFlags))
                {
                    releaseInfo.IndexerFlags = indexerFlags;
                }

                //Now we run the release info augmenters from the history release info. TODO: Add setting to only do that if you trust your indexer!
                var releaseInfoAugmenters = _augmenters.Where(a => a.HelperType == typeof(ReleaseInfo));
                foreach (var augmenter in releaseInfoAugmenters)
                {
                    movieInfo = augmenter.AugmentMovieInfo(movieInfo, releaseInfo);
                }
            }

            return movieInfo;
        }
    }
}
