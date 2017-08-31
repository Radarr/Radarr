using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public class ManuallyImportedFile
    {
        public TrackedDownload TrackedDownload { get; set; }
        public ImportResult ImportResult { get; set; }
    }
}
