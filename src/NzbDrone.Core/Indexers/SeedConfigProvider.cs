using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface ISeedConfigProvider
    {
        TorrentSeedConfiguration GetSeedConfiguration(RemoteBook release);
    }

    public class SeedConfigProvider : ISeedConfigProvider
    {
        private readonly IIndexerFactory _indexerFactory;

        public SeedConfigProvider(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public TorrentSeedConfiguration GetSeedConfiguration(RemoteBook remoteBook)
        {
            if (remoteBook.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return null;
            }

            if (remoteBook.Release.IndexerId == 0)
            {
                return null;
            }

            try
            {
                var indexer = _indexerFactory.Get(remoteBook.Release.IndexerId);
                var torrentIndexerSettings = indexer.Settings as ITorrentIndexerSettings;

                if (torrentIndexerSettings != null && torrentIndexerSettings.SeedCriteria != null)
                {
                    var seedConfig = new TorrentSeedConfiguration
                    {
                        Ratio = torrentIndexerSettings.SeedCriteria.SeedRatio
                    };

                    var seedTime = remoteBook.ParsedBookInfo.Discography ? torrentIndexerSettings.SeedCriteria.DiscographySeedTime : torrentIndexerSettings.SeedCriteria.SeedTime;
                    if (seedTime.HasValue)
                    {
                        seedConfig.SeedTime = TimeSpan.FromMinutes(seedTime.Value);
                    }

                    return seedConfig;
                }
            }
            catch (ModelNotFoundException)
            {
                return null;
            }

            return null;
        }
    }
}
