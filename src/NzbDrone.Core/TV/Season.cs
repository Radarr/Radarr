using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.TV
{
    public class Season : ModelBase
    {
        public int TVShowId { get; set; }
        public int SeasonNumber { get; set; }

        public string Title { get; set; }
        public string Overview { get; set; }

        public bool Monitored { get; set; }

        public override string ToString()
        {
            return SeasonNumber == 0 ? "Specials" : $"Season {SeasonNumber}";
        }
    }
}
