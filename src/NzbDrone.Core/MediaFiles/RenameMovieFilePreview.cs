namespace NzbDrone.Core.MediaFiles
{
    public class RenameMovieFilePreview
    {
        public int MovieId { get; set; }
        public int MovieFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
