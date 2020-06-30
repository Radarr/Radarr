using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class WorkResource
    {
        public int GoodreadsId { get; set; }
        public string Title { get; set; }
        public string TitleSlug { get; set; }
        public string Url { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<BookResource> Books { get; set; } = new List<BookResource>();
    }
}
