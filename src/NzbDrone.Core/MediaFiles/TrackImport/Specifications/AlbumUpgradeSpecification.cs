using System;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class AlbumUpgradeSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly Logger _logger;

        public AlbumUpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease)
        {
            var artist = localAlbumRelease.AlbumRelease.Album.Value.Artist.Value;
            var qualityComparer = new QualityModelComparer(artist.QualityProfile);

            // check if we are changing release
            var currentRelease = localAlbumRelease.AlbumRelease.Album.Value.AlbumReleases.Value.Single(x => x.Monitored);
            var newRelease = localAlbumRelease.AlbumRelease;

            // if we are, check we are upgrading
            if (newRelease.Id != currentRelease.Id)
            {
                // min quality of all new tracks
                var newMinQuality = localAlbumRelease.LocalTracks.Select(x => x.Quality).OrderBy(x => x, qualityComparer).First();
                _logger.Debug("Min quality of new files: {0}", newMinQuality);
                
                // get minimum quality of existing release
                var existingQualities = currentRelease.Tracks.Value.Where(x => x.TrackFileId != 0).Select(x => x.TrackFile.Value.Quality);
                if (existingQualities.Any())
                {
                    var existingMinQuality = existingQualities.OrderBy(x => x, qualityComparer).First();
                    _logger.Debug("Min quality of existing files: {0}", existingMinQuality);
                    if (qualityComparer.Compare(existingMinQuality, newMinQuality) > 0)
                    {
                        _logger.Debug("This album isn't a quality upgrade for all tracks. Skipping {0}", localAlbumRelease);
                        return Decision.Reject("Not an upgrade for existing album file(s)");
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
