using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class CloseTrackMatchSpecification : IImportDecisionEngineSpecification<LocalTrack>
    {
        private const double _threshold = 0.4;
        private readonly Logger _logger;

        public CloseTrackMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalTrack item, DownloadClientItem downloadClientItem)
        {
            var dist = item.Distance.NormalizedDistance();
            var reasons = item.Distance.Reasons;

            if (dist > _threshold)
            {
                _logger.Debug($"Track match is not close enough: {dist} vs {_threshold} {reasons}. Skipping {item}");
                return Decision.Reject($"Track match is not close enough: {1 - dist:P1} vs {1 - _threshold:P0} {reasons}");
            }

            _logger.Debug($"Track accepted: {dist} vs {_threshold} {reasons}.");
            return Decision.Accept();
        }
    }
}
