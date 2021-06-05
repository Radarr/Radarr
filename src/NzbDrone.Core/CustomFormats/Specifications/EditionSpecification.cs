using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class EditionSpecification : RegexSpecificationBase
    {
        public override int Order => 2;
        public override string ImplementationName => "Edition";
        public override string InfoLink => "https://wiki.servarr.com/radarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            return MatchString(movieInfo.Edition);
        }
    }
}
