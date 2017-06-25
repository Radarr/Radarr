using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class Ratings : IEmbeddedDocument
    {
        public int Votes { get; set; }
        public decimal Value { get; set; }
    }
}
