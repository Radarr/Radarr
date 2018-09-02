using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public interface IAugmentParsedMovieInfo
    {
        Type HelperType { get; }

        ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper);
    }
}
