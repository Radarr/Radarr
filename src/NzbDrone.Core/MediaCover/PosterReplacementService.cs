using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public interface IPosterReplacementService
    {
        // Returns null when the movie's poster should not be replaced
        PosterReplacement GetReplacement(Movie movie);
        bool ShouldReplace(IEnumerable<string> genres, IEnumerable<int> tags);
    }

    public class PosterReplacement
    {
        // Bump when the rendered output changes so existing replacement posters are regenerated
        private const int RenderVersion = 1;

        public string Title { get; }
        public string BackgroundColor { get; }
        public string TextColor { get; }

        public PosterReplacement(string title, string backgroundColor, string textColor)
        {
            Title = title ?? string.Empty;
            BackgroundColor = backgroundColor;
            TextColor = textColor;
        }

        // Identifies the rendered image, used to detect when it needs to be regenerated and to bust the browser cache
        public string Signature => $"{RenderVersion}|{BackgroundColor}|{TextColor}|{Title}".SHA256Hash();
    }

    public class PosterReplacementService : IPosterReplacementService
    {
        private readonly IConfigService _configService;

        public PosterReplacementService(IConfigService configService)
        {
            _configService = configService;
        }

        public PosterReplacement GetReplacement(Movie movie)
        {
            var metadata = movie.MovieMetadata?.Value;

            if (metadata == null || !ShouldReplace(metadata.Genres, movie.Tags))
            {
                return null;
            }

            return new PosterReplacement(metadata.Title,
                                         _configService.PosterReplacementBackgroundColor,
                                         _configService.PosterReplacementTextColor);
        }

        public bool ShouldReplace(IEnumerable<string> genres, IEnumerable<int> tags)
        {
            if (!_configService.PosterReplacementEnabled)
            {
                return false;
            }

            var replacedGenres = _configService.PosterReplacementGenres;

            if (genres != null && replacedGenres.Any() && genres.Any(g => replacedGenres.Contains(g, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            var replacedTags = _configService.PosterReplacementTags;

            return tags != null && replacedTags.Any() && tags.Intersect(replacedTags).Any();
        }
    }
}
