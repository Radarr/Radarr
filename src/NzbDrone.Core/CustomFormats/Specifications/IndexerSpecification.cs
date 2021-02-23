using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class IndexerSpecification : RegexSpecificationBase
    {
        public override int Order => 9;
        public override string ImplementationName => "Indexer Name";

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            var indexerName = movieInfo?.ExtraInfo?.GetValueOrDefault("IndexerName") as string;
            return MatchString(indexerName);
        }
    }
}
