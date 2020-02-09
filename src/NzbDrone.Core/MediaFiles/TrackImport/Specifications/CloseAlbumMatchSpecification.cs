using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class CloseAlbumMatchSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private const double _albumThreshold = 0.20;
        private const double _trackThreshold = 0.40;
        private readonly Logger _logger;

        public CloseAlbumMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease item, DownloadClientItem downloadClientItem)
        {
            double dist;
            string reasons;

            // strict when a new download
            if (item.NewDownload)
            {
                dist = item.Distance.NormalizedDistance();
                reasons = item.Distance.Reasons;
                if (dist > _albumThreshold)
                {
                    _logger.Debug($"Album match is not close enough: {dist} vs {_albumThreshold} {reasons}. Skipping {item}");
                    return Decision.Reject($"Album match is not close enough: {1 - dist:P1} vs {1 - _albumThreshold:P0} {reasons}");
                }

                var worstTrackMatch = item.LocalTracks.Where(x => x.Distance != null).OrderByDescending(x => x.Distance.NormalizedDistance()).FirstOrDefault();
                if (worstTrackMatch == null)
                {
                    _logger.Debug($"No tracks matched");
                    return Decision.Reject("No tracks matched");
                }
                else
                {
                    var maxTrackDist = worstTrackMatch.Distance.NormalizedDistance();
                    var trackReasons = worstTrackMatch.Distance.Reasons;
                    if (maxTrackDist > _trackThreshold)
                    {
                        _logger.Debug($"Worst track match: {maxTrackDist} vs {_trackThreshold} {trackReasons}. Skipping {item}");
                        return Decision.Reject($"Worst track match: {1 - maxTrackDist:P1} vs {1 - _trackThreshold:P0} {trackReasons}");
                    }
                }
            }

            // otherwise importing existing files in library
            else
            {
                // get album distance ignoring whether tracks are missing
                dist = item.Distance.NormalizedDistanceExcluding(new List<string> { "missing_tracks", "unmatched_tracks" });
                reasons = item.Distance.Reasons;
                if (dist > _albumThreshold)
                {
                    _logger.Debug($"Album match is not close enough: {dist} vs {_albumThreshold} {reasons}. Skipping {item}");
                    return Decision.Reject($"Album match is not close enough: {1 - dist:P1} vs {1 - _albumThreshold:P0} {reasons}");
                }
            }

            _logger.Debug($"Accepting release {item}: dist {dist} vs {_albumThreshold} {reasons}");
            return Decision.Accept();
        }
    }
}
