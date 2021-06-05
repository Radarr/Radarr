using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseTitleSpecification : RegexSpecificationBase
    {
        public override int Order => 1;
        public override string ImplementationName => "Release Title";
        public override string InfoLink => "https://wiki.servarr.com/radarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            var filename = (string)movieInfo?.ExtraInfo?.GetValueOrDefault("Filename");

            return MatchString(movieInfo?.SimpleReleaseTitle) || MatchString(filename);
        }
    }
}
