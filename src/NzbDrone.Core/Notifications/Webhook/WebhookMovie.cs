using System.IO;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string FilePath { get; set; }
        public string ReleaseDate { get; set; }
        public string FolderPath { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }

        public WebhookMovie()
        {
        }

        public WebhookMovie(Movie movie)
        {
            Id = movie.Id;
            Title = movie.Title;
            Year = movie.Year;
            ReleaseDate = movie.MovieMetadata.Value.PhysicalReleaseDate().ToString("yyyy-MM-dd");
            FolderPath = movie.Path;
            TmdbId = movie.TmdbId;
            ImdbId = movie.ImdbId;
        }

        public WebhookMovie(Movie movie, MovieFile movieFile)
            : this(movie)
        {
            FilePath = Path.Combine(movie.Path, movieFile.RelativePath);
        }
    }
}
