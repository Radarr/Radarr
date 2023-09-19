using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImportParser : RssImportBaseParser
    {
        public PlexRssImportParser(Logger logger)
            : base(logger)
        {
        }

        protected override ImportListMovie ProcessItem(XElement item)
        {
            var category = item.TryGetValue("category");

            if (category != "movie")
            {
                return null;
            }

            var info = new ImportListMovie
            {
                Title = item.TryGetValue("title", "Unknown")
            };

            var guid = item.TryGetValue("guid", string.Empty);

            if (guid.IsNotNullOrWhiteSpace())
            {
                if (guid.StartsWith("imdb://"))
                {
                    info.ImdbId = Parser.Parser.ParseImdbId(guid.Replace("imdb://", ""));
                }

                if (int.TryParse(guid.Replace("tmdb://", ""), out var tmdbId))
                {
                    info.TmdbId = tmdbId;
                }
            }

            if (info.ImdbId.IsNullOrWhiteSpace() && info.TmdbId == 0)
            {
                throw new UnsupportedFeedException("Each item in the RSS feed must have a guid element with a IMDB ID or TMDB ID");
            }

            return info;
        }
    }
}
