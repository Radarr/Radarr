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
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public delegate int CompareDelegate(DownloadDecision x, DownloadDecision y);
        public delegate int CompareDelegate<TSubject, TValue>(DownloadDecision x, DownloadDecision y);

        public DownloadDecisionComparer(IConfigService configService, IDelayProfileService delayProfileService, IQualityDefinitionService qualityDefinitionService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
            _qualityDefinitionService = qualityDefinitionService;
        }

        public int Compare(DownloadDecision x, DownloadDecision y)
        {
            var comparers = new List<CompareDelegate>
            {
                CompareQuality,
                CompareCustomFormatScore,
                CompareProtocol,
                CompareIndexerPriority,
                CompareIndexerFlags,
                ComparePeersIfTorrent,
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

        private int CompareIndexerPriority(DownloadDecision x, DownloadDecision y)
        {
            return CompareByReverse(x.RemoteMovie.Release, y.RemoteMovie.Release, release => release.IndexerPriority);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.Movie.Profile.GetIndex(remoteMovie.ParsedMovieInfo.Quality.Quality));
            }

            return CompareAll(CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.Movie.Profile.GetIndex(remoteMovie.ParsedMovieInfo.Quality.Quality)),
                              CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.ParsedMovieInfo.Quality.Revision));
        }

        private int CompareCustomFormatScore(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.CustomFormatScore);
        }

        private int CompareIndexerFlags(DownloadDecision x, DownloadDecision y)
        {
            var releaseX = x.RemoteMovie.Release;
            var releaseY = y.RemoteMovie.Release;

            if (_configService.PreferIndexerFlags)
            {
                return CompareBy(x.RemoteMovie.Release, y.RemoteMovie.Release, release => ScoreFlags(release.IndexerFlags));
            }
            else
            {
                return 0;
            }
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var result = CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteMovie.Movie.Tags);
                var downloadProtocol = remoteMovie.Release.DownloadProtocol;
                return downloadProtocol == delayProfile.PreferredProtocol;
            });

            return result;
        }

        private int ComparePeersIfTorrent(DownloadDecision x, DownloadDecision y)
        {
            // Different protocols should get caught when checking the preferred protocol,
            // since we're dealing with the same movie in our comparisions
            if (x.RemoteMovie.Release.DownloadProtocol != DownloadProtocol.Torrent ||
                y.RemoteMovie.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return 0;
            }

            return CompareAll(
                CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
                {
                    var seeders = TorrentInfo.GetSeeders(remoteMovie.Release);

                    return seeders.HasValue && seeders.Value > 0 ? Math.Round(Math.Log10(seeders.Value)) : 0;
                }),
                CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
                {
                    var peers = TorrentInfo.GetPeers(remoteMovie.Release);

                    return peers.HasValue && peers.Value > 0 ? Math.Round(Math.Log10(peers.Value)) : 0;
                }));
        }

        private int CompareAgeIfUsenet(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteMovie.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteMovie.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
            {
                var ageHours = remoteMovie.Release.AgeHours;
                var age = remoteMovie.Release.Age;

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
            var sizeCompare =  CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
            {
                var preferredSize = _qualityDefinitionService.Get(remoteMovie.ParsedMovieInfo.Quality.Quality).PreferredSize;

                // If no value for preferred it means unlimited so fallback to sort largest is best
                if (preferredSize.HasValue && remoteMovie.Movie.MovieMetadata.Value.Runtime > 0)
                {
                    var preferredMovieSize = remoteMovie.Movie.MovieMetadata.Value.Runtime * preferredSize.Value.Megabytes();

                    // Calculate closest to the preferred size
                    return Math.Abs((remoteMovie.Release.Size - preferredMovieSize).Round(200.Megabytes())) * (-1);
                }
                else
                {
                    return remoteMovie.Release.Size.Round(200.Megabytes());
                }
            });

            return sizeCompare;
        }

        private int ScoreFlags(IndexerFlags flags)
        {
            var flagValues = Enum.GetValues(typeof(IndexerFlags));

            var score = 0;

            foreach (IndexerFlags value in flagValues)
            {
                if ((flags & value) == value)
                {
                    switch (value)
                    {
                        case IndexerFlags.G_DoubleUpload:
                        case IndexerFlags.G_Freeleech:
                        case IndexerFlags.PTP_Approved:
                        case IndexerFlags.PTP_Golden:
                        case IndexerFlags.HDB_Internal:
                        case IndexerFlags.AHD_Internal:
                            score += 2;
                            break;
                        case IndexerFlags.G_Halfleech:
                        case IndexerFlags.AHD_UserRelease:
                            score += 1;
                            break;
                    }
                }
            }

            return score;
        }
    }
}
