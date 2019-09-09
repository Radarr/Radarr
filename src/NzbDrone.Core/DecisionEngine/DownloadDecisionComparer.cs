using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecisionComparer : IComparer<DownloadDecision>
    {
        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;

        public delegate int CompareDelegate(DownloadDecision x, DownloadDecision y);
        public delegate int CompareDelegate<TSubject, TValue>(DownloadDecision x, DownloadDecision y);

        public DownloadDecisionComparer(IConfigService configService, IDelayProfileService delayProfileService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
        }

        public int Compare(DownloadDecision x, DownloadDecision y)
        {
            var comparers = new List<CompareDelegate>
            {
                CompareQuality,
                ComparePreferredWordScore,
                CompareProtocol,
                ComparePeersIfTorrent,
                CompareAlbumCount,
                CompareAgeIfUsenet,
                CompareSize
            };

            return comparers.Select(comparer => comparer(x, y)).FirstOrDefault(result => result != 0);
        }

        private int CompareBy<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            var leftValue = funcValue(left);
            var rightValue = funcValue(right);

            return leftValue.CompareTo(rightValue);
        }

        private int CompareByReverse<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            return CompareBy(left, right, funcValue) * -1;
        }

        private int CompareAll(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                return CompareAll(CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.Artist.QualityProfile.Value.GetIndex(remoteAlbum.ParsedAlbumInfo.Quality.Quality)),
                    CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.ParsedAlbumInfo.Quality.Revision.Real));
            }

            return CompareAll(CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.Artist.QualityProfile.Value.GetIndex(remoteAlbum.ParsedAlbumInfo.Quality.Quality)),
                           CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.ParsedAlbumInfo.Quality.Revision.Real),
                           CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.ParsedAlbumInfo.Quality.Revision.Version));
        }

        private int ComparePreferredWordScore(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.PreferredWordScore);
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var result = CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteAlbum.Artist.Tags);
                var downloadProtocol = remoteAlbum.Release.DownloadProtocol;
                return downloadProtocol == delayProfile.PreferredProtocol;
            });

            return result;
        }

        private int CompareAlbumCount(DownloadDecision x, DownloadDecision y)
        {
            var discographyCompare = CompareBy(x.RemoteAlbum, y.RemoteAlbum,
                remoteAlbum => remoteAlbum.ParsedAlbumInfo.Discography);

            if (discographyCompare != 0)
            {
                return discographyCompare;
            }

            return CompareByReverse(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.Albums.Count);
        }

        private int ComparePeersIfTorrent(DownloadDecision x, DownloadDecision y)
        {
            // Different protocols should get caught when checking the preferred protocol,
            // since we're dealing with the same series in our comparisions
            if (x.RemoteAlbum.Release.DownloadProtocol != DownloadProtocol.Torrent ||
                y.RemoteAlbum.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return 0;
            }

            return CompareAll(
                CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum =>
                {
                    var seeders = TorrentInfo.GetSeeders(remoteAlbum.Release);

                    return seeders.HasValue && seeders.Value > 0 ? Math.Round(Math.Log10(seeders.Value)) : 0;
                }),
                CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum =>
                {
                    var peers = TorrentInfo.GetPeers(remoteAlbum.Release);

                    return peers.HasValue && peers.Value > 0 ? Math.Round(Math.Log10(peers.Value)) : 0;
                }));
        }

        private int CompareAgeIfUsenet(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteAlbum.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteAlbum.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            return CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum =>
            {
                var ageHours = remoteAlbum.Release.AgeHours;
                var age = remoteAlbum.Release.Age;

                if (ageHours < 1)
                {
                    return 1000;
                }

                if (ageHours <= 24)
                {
                    return 100;
                }

                if (age <= 7)
                {
                    return 10;
                }

                return 1;
            });
        }

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            // TODO: Is smaller better? Smaller for usenet could mean no par2 files.

            return CompareBy(x.RemoteAlbum, y.RemoteAlbum, remoteAlbum => remoteAlbum.Release.Size.Round(200.Megabytes()));
        }
    }
}
