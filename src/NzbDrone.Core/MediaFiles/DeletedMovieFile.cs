namespace NzbDrone.Core.MediaFiles
{
    public class DeletedMovieFile
    {
        public string RecycleBinPath { get; set; }
        public MovieFile MovieFile { get; set; }

        public DeletedMovieFile(MovieFile movieFile, string recycleBinPath)
        {
            MovieFile = movieFile;
            RecycleBinPath = recycleBinPath;
        }
    }
}
