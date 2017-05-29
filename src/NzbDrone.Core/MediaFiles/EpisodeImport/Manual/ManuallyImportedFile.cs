using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.TrackImport;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Manual
{
    public class ManuallyImportedFile
    {
        public TrackedDownload TrackedDownload { get; set; }
        public ImportResult ImportResult { get; set; }
    }
}
