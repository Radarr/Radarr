using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public class ManuallyImportedFile
    {
        public TrackedDownload TrackedDownload { get; set; }
        public ImportResult ImportResult { get; set; }
    }
}
