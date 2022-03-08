using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class MovieDeleteMessage
    {
        public string Message { get; set; }
        public Movie Movie { get; set; }
        public bool DeletedFiles { get; set; }
        public string DeletedFilesMessage { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public MovieDeleteMessage(Movie movie, bool deleteFiles)
        {
            Movie = movie;
            DeletedFiles = deleteFiles;
            DeletedFilesMessage = DeletedFiles ?
                "Movie removed and all files were deleted" :
                "Movie removed, files were not deleted";
            Message = movie.Title + " - " + DeletedFilesMessage;
        }
    }
}
