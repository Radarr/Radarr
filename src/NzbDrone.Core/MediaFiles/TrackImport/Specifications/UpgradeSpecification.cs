using System;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalTrack localTrack)
        {
            var qualityComparer = new QualityModelComparer(localTrack.Artist.Profile);
            var languageComparer = new LanguageComparer(localTrack.Artist.LanguageProfile);
            var profile = localTrack.Artist.Profile.Value;

            if (localTrack.Tracks.Any(e => e.TrackFileId != 0 &&
                                      languageComparer.Compare(e.TrackFile.Value.Language, localTrack.Language) > 0 &&
                                      qualityComparer.Compare(e.TrackFile.Value.Quality, localTrack.Quality) == 0))
            {
                _logger.Debug("This file isn't an upgrade for all tracks. Skipping {0}", localTrack.Path);
                return Decision.Reject("Not an upgrade for existing track file(s)");
            }

            return Decision.Accept();
        }
    }
}
