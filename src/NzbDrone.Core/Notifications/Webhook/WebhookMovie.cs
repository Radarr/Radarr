using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string Overview { get; set; }
        public List<string> Genres { get; set; }
        public List<WebhookImage> Images { get; set; }
        public List<string> Tags { get; set; }

        public WebhookMovie()
        {
        }

        public WebhookMovie(Movie movie, List<string> tags)
        {
            Id = movie.Id;
            Title = movie.Title;
            Year = movie.Year;
            ReleaseDate = movie.MovieMetadata.Value.PhysicalReleaseDate().ToString("yyyy-MM-dd");
            FolderPath = movie.Path;
            TmdbId = movie.TmdbId;
            ImdbId = movie.ImdbId;
            Overview = movie.MovieMetadata.Value.Overview;
            Genres = movie.MovieMetadata.Value.Genres;
            Images = movie.MovieMetadata.Value.Images.Select(i => new WebhookImage(i)).ToList();
            Tags = tags;
        }

        public WebhookMovie(Movie movie, MovieFile movieFile, List<string> tags)
            : this(movie, tags)
        {
            FilePath = Path.Combine(movie.Path, movieFile.RelativePath);
        }
    }
}
