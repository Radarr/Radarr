using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download.Clients;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteBook
    {
        public ReleaseInfo Release { get; set; }
        public ParsedBookInfo ParsedBookInfo { get; set; }
        public Author Author { get; set; }
        public List<Book> Books { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public int PreferredWordScore { get; set; }

        public RemoteBook()
        {
            Books = new List<Book>();
        }

        public bool IsRecentBook()
        {
            return Books.Any(e => e.ReleaseDate >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}
