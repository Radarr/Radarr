using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DeletedTrackFileSpecification : IDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IMediaFileService _albumService;
        private readonly Logger _logger;

        public DeletedTrackFileSpecification(IDiskProvider diskProvider,
                                             IConfigService configService,
                                             IMediaFileService albumService,
                                             Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _albumService = albumService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Disk;
        public RejectionType Type => RejectionType.Temporary;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (!_configService.AutoUnmonitorPreviouslyDownloadedTracks)
            {
                return Decision.Accept();
            }

            if (searchCriteria != null)
            {
                _logger.Debug("Skipping deleted trackfile check during search");
                return Decision.Accept();
            }

            var missingTrackFiles = subject.Albums
                                             .SelectMany(v => _albumService.GetFilesByAlbum(v.Id))
                                             .DistinctBy(v => v.Id)
                                             .Where(v => IsTrackFileMissing(subject.Artist, v))
                                             .ToArray();


            if (missingTrackFiles.Any())
            {
                foreach (var missingTrackFile in missingTrackFiles)
                {
                    _logger.Trace("Track file {0} is missing from disk.", missingTrackFile.Path);
                }

                _logger.Debug("Files for this album exist in the database but not on disk, will be unmonitored on next diskscan. skipping.");
                return Decision.Reject("Artist is not monitored");
            }

            return Decision.Accept();
        }

        private bool IsTrackFileMissing(Artist artist, TrackFile trackFile)
        {
            return !_diskProvider.FileExists(trackFile.Path);
        }
    }
}
