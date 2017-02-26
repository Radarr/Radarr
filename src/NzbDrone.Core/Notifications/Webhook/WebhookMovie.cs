using System.IO;
﻿using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }

        public WebhookMovie() { }

        public WebhookMovie(Movie movie)
        {
            Id = movie.Id;
            Title = movie.Title;
        }

        public WebhookMovie(Movie movie, MovieFile movieFile)
        {
            Id = movie.Id;
            Title = movie.Title;
            FilePath = Path.Combine(movie.Path, movieFile.RelativePath);
        }
    }
}
