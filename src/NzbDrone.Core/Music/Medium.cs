using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class Medium : IEmbeddedDocument
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
    }
}
