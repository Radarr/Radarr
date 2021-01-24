using System.Collections.Generic;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Specifications
{
    public class CloseBookMatchSpecification : IImportDecisionEngineSpecification<LocalEdition>
    {
        private const double _bookThreshold = 0.20;
        private readonly Logger _logger;

        public CloseBookMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEdition item, DownloadClientItem downloadClientItem)
        {
            double dist;
            string reasons;

            // strict when a new download
            if (item.NewDownload)
            {
                dist = item.Distance.NormalizedDistance();
                reasons = item.Distance.Reasons;
                if (dist > _bookThreshold)
                {
                    _logger.Debug($"Book match is not close enough: {dist} vs {_bookThreshold} {reasons}. Skipping {item}");
                    return Decision.Reject($"Book match is not close enough: {1 - dist:P1} vs {1 - _bookThreshold:P0} {reasons}");
                }
            }

            // otherwise importing existing files in library
            else
            {
                // get book distance ignoring whether tracks are missing
                dist = item.Distance.NormalizedDistanceExcluding(new List<string> { "missing_tracks", "unmatched_tracks" });
                reasons = item.Distance.Reasons;
                if (dist > _bookThreshold)
                {
                    _logger.Debug($"Book match is not close enough: {dist} vs {_bookThreshold} {reasons}. Skipping {item}");
                    return Decision.Reject($"Book match is not close enough: {1 - dist:P1} vs {1 - _bookThreshold:P0} {reasons}");
                }
            }

            _logger.Debug($"Accepting release {item}: dist {dist} vs {_bookThreshold} {reasons}");
            return Decision.Accept();
        }
    }
}
