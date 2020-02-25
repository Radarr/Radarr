using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;

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
                ComparePreferredWords,
                CompareIndexerFlags,
                CompareProtocol,
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

        private int CompareAll(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            return CompareAll(CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.Movie.Profile.GetIndex(remoteMovie.ParsedMovieInfo.Quality.Quality)),
                              CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.CustomFormatScore),
                              CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.ParsedMovieInfo.Quality.Revision.Real),
                              CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie => remoteMovie.ParsedMovieInfo.Quality.Revision.Version));
        }

        private int ComparePreferredWords(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteMovie =>
            {
                var title = remoteMovie.Release.Title;
                var preferredWords = remoteMovie.Movie.Profile.PreferredTags;

                if (preferredWords == null)
                {
                    return 0;
                }

                var num = preferredWords.AsEnumerable().Count(w => title.ToLower().Contains(w.ToLower()));

                return num;
            });
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
            var result = CompareBy(x.RemoteMovie, y.RemoteMovie, remoteEpisode =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteEpisode.Movie.Tags);
                var downloadProtocol = remoteEpisode.Release.DownloadProtocol;
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
                CompareBy(x.RemoteMovie, y.RemoteMovie, remoteEpisode =>
                {
                    var seeders = TorrentInfo.GetSeeders(remoteEpisode.Release);

                    return seeders.HasValue && seeders.Value > 0 ? Math.Round(Math.Log10(seeders.Value)) : 0;
                }),
                CompareBy(x.RemoteMovie, y.RemoteMovie, remoteEpisode =>
                {
                    var peers = TorrentInfo.GetPeers(remoteEpisode.Release);

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

            return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteEpisode =>
            {
                var ageHours = remoteEpisode.Release.AgeHours;
                var age = remoteEpisode.Release.Age;

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
            return CompareBy(x.RemoteMovie, y.RemoteMovie, remoteEpisode => remoteEpisode.Release.Size.Round(200.Megabytes()));
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
                            score += 2;
                            break;
                        case IndexerFlags.G_Halfleech:
                            score += 1;
                            break;
                    }
                }
            }

            return score;
        }
    }
}
