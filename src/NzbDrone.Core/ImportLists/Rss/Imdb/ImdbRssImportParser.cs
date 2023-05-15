using System;
using System.Xml.Linq;
using NLog;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.Rss.Imdb
{
    public class ImdbRssImportParser : RssImportBaseParser
    {
        public ImdbRssImportParser(Logger logger)
            : base(logger)
        {
        }

        protected override ImportListMovie ProcessItem(XElement item)
        {
            throw new NotImplementedException();
        }
    }
}
