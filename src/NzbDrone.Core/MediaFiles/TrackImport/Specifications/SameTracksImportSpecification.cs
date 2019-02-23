using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class SameTracksImportSpecification : IImportDecisionEngineSpecification<LocalTrack>
    {
        private readonly SameTracksSpecification _sameTracksSpecification;
        private readonly Logger _logger;

        public SameTracksImportSpecification(SameTracksSpecification sameTracksSpecification, Logger logger)
        {
            _sameTracksSpecification = sameTracksSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(LocalTrack localTrack)
        {
            if (_sameTracksSpecification.IsSatisfiedBy(localTrack.Tracks))
            {
                return Decision.Accept();
            }

            _logger.Debug("Track file on disk contains more tracks than this file contains");
            return Decision.Reject("Track file on disk contains more tracks than this file contains");
        }
    }
}
