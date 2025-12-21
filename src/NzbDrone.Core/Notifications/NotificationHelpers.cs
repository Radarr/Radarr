using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public static class NotificationHelpers
    {
        /// <summary>
        /// Converts a byte count to a human-readable string (e.g., "1.5 GB")
        /// </summary>
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0} {1}", (Math.Sign(byteCount) * num).ToString(), suf[place]);
        }

        /// <summary>
        /// Creates a string of markdown-formatted links for a movie's metadata sources
        /// </summary>
        public static string GetLinksString(Movie movie)
        {
            if (movie?.MovieMetadata?.Value == null)
            {
                return string.Empty;
            }

            var links = new List<string>
            {
                $"[TMDb](https://themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId})",
                $"[Trakt](https://trakt.tv/search/tmdb/{movie.MovieMetadata.Value.TmdbId}?id_type=movie)"
            };

            if (movie.MovieMetadata.Value.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add($"[IMDb](https://imdb.com/title/{movie.MovieMetadata.Value.ImdbId}/)");
            }

            if (movie.MovieMetadata.Value.YouTubeTrailerId.IsNotNullOrWhiteSpace())
            {
                links.Add($"[YouTube](https://www.youtube.com/watch?v={movie.MovieMetadata.Value.YouTubeTrailerId})");
            }

            if (movie.MovieMetadata.Value.Website.IsNotNullOrWhiteSpace())
            {
                links.Add($"[Website]({movie.MovieMetadata.Value.Website})");
            }

            return string.Join(" / ", links);
        }

        /// <summary>
        /// Gets a formatted movie title with year, suitable for notifications
        /// </summary>
        public static string GetTitle(Movie movie)
        {
            if (movie == null)
            {
                return string.Empty;
            }

            var title = (movie.MovieMetadata.Value.Year > 0 
                ? $"{movie.MovieMetadata.Value.Title} ({movie.MovieMetadata.Value.Year})" 
                : movie.MovieMetadata.Value.Title).Replace("`", "\\`");

            return title.Length > 256 ? $"{title.AsSpan(0, 253).TrimEnd('\\')}..." : title;
        }

        /// <summary>
        /// Gets a common "movie added to library" message
        /// </summary>
        public static string GetMovieAddedMessage(Movie movie)
        {
            if (movie == null)
            {
                return string.Empty;
            }

            return $"{movie.Title} added to library";
        }

        /// <summary>
        /// Gets a common "health issue resolved" message
        /// </summary>
        public static string GetHealthRestoredMessage(HealthCheck.HealthCheck previousCheck)
        {
            if (previousCheck == null)
            {
                return string.Empty;
            }

            return $"The following issue is now resolved: {previousCheck.Message}";
        }
    }
}
