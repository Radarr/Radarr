using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications;

public static class NotificationMetadataLinkGenerator
{
    public static List<NotificationMetadataLink> GenerateLinks(Movie movie, IEnumerable<int> metadataLinks)
    {
        var links = new List<NotificationMetadataLink>();

        if (movie == null)
        {
            return links;
        }

        foreach (var type in metadataLinks)
        {
            var linkType = (MetadataLinkType)type;

            if (linkType == MetadataLinkType.Imdb && movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Imdb, "IMDb", $"https://www.imdb.com/title/{movie.ImdbId}"));
            }

            if (linkType == MetadataLinkType.Tmdb && movie.TmdbId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Tmdb, "TMDb", $"https://www.themoviedb.org/movie/{movie.TmdbId}"));
            }
        }

        return links;
    }
}
