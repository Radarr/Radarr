using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class Member : IEmbeddedDocument
    {
        public Member()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public string Instrument { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
