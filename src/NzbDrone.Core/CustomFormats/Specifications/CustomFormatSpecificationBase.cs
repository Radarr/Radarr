using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public abstract class CustomFormatSpecificationBase : ICustomFormatSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public virtual string InfoLink => "https://github.com/Radarr/Radarr/wiki/Custom-Formats-Aphrodite";

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }

        public ICustomFormatSpecification Clone()
        {
            return (ICustomFormatSpecification)MemberwiseClone();
        }

        public bool IsSatisfiedBy(ParsedMovieInfo movieInfo)
        {
            var match = IsSatisfiedByWithoutNegate(movieInfo);
            if (Negate)
            {
                match = !match;
            }

            return match;
        }

        protected abstract bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo);
    }
}
