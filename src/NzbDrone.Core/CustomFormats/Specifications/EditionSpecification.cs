using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class EditionSpecification : RegexSpecificationBase
    {
        public override int Order => 2;
        public override string ImplementationName => "Edition";
        public override string InfoLink => "https://github.com/Radarr/Radarr/wiki/Custom-Formats-Aphrodite#edition";

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            return MatchString(movieInfo.Edition);
        }
    }
}
