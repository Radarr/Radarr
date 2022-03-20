using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies
{
    public class Ratings : MemberwiseEquatable<Ratings>, IEmbeddedDocument
    {
        public RatingChild Imdb { get; set; }
        public RatingChild Tmdb { get; set; }
        public RatingChild Metacritic { get; set; }
        public RatingChild RottenTomatoes { get; set; }
    }

    public class RatingChild
    {
        public int Votes { get; set; }
        public decimal Value { get; set; }
        public RatingType Type { get; set; }
    }

    public enum RatingType
    {
        User,
        Critic
    }
}
