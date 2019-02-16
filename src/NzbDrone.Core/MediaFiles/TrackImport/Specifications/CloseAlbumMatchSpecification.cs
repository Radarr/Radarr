using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class CloseAlbumMatchSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly Logger _logger;
        private const double _albumThreshold = 0.20;
        private const double _trackThreshold = 0.40;

        public CloseAlbumMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease)
        {
            double dist;
            string reasons;
            
            // strict when a new download
            if (localAlbumRelease.NewDownload)
            {
                dist = localAlbumRelease.Distance.NormalizedDistance();
                reasons = localAlbumRelease.Distance.Reasons;
                if (dist > _albumThreshold)
                {
                    _logger.Debug($"Album match is not close enough: {dist} vs {_albumThreshold} {reasons}. Skipping {localAlbumRelease}");
                    return Decision.Reject($"Album match is not close enough: {1-dist:P1} vs {1-_albumThreshold:P0} {reasons}");
                }

                var worstTrackMatch = localAlbumRelease.LocalTracks.Where(x => x.Distance != null).OrderByDescending(x => x.Distance.NormalizedDistance()).FirstOrDefault();
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
                        _logger.Debug($"Worst track match: {maxTrackDist} vs {_trackThreshold} {trackReasons}. Skipping {localAlbumRelease}");
                        return Decision.Reject($"Worst track match: {1-maxTrackDist:P1} vs {1-_trackThreshold:P0} {trackReasons}");
                    }
                }
            }
            // otherwise importing existing files in library
            else
            {
                // get album distance ignoring whether tracks are missing
                dist = localAlbumRelease.Distance.NormalizedDistanceExcluding(new List<string> { "missing_tracks", "unmatched_tracks" });
                reasons = localAlbumRelease.Distance.Reasons;
                if (dist > _albumThreshold)
                {
                    _logger.Debug($"Album match is not close enough: {dist} vs {_albumThreshold} {reasons}. Skipping {localAlbumRelease}");
                    return Decision.Reject($"Album match is not close enough: {1-dist:P1} vs {1-_albumThreshold:P0} {reasons}");
                }
            }

            _logger.Debug($"Accepting release {localAlbumRelease}: dist {dist} vs {_albumThreshold} {reasons}");
            return Decision.Accept();
        }
    }
}
