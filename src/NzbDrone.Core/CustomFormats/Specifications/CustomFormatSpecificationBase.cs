using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public abstract class CustomFormatSpecificationBase : ICustomFormatSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public virtual string InfoLink => "https://wiki.servarr.com/radarr/settings#custom-formats-2";

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
