using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser
{
    public static class SceneChecker
    {
        //This method should prefer false negatives over false positives.
        //It's better not to use a title that might be scene than to use one that isn't scene
        public static bool IsSceneTitle(string title)
        {
            if (!title.Contains(".")) return false;
            if (title.Contains(" ")) return false;

            var parsedTitle = Parser.ParseMovieTitle(title, false); //We are not lenient when it comes to scene checking!
            var rlsGroup = Parser.ParseReleaseGroup(title);
            var quality = QualityParser.ParseQuality(title);

            if (parsedTitle == null ||
                rlsGroup == null ||
                string.IsNullOrWhiteSpace(parsedTitle.MovieTitle) ||
                quality.Resolution == Resolution.Unknown ||
                quality.Source == Source.UNKNOWN)
            {
                return false;
            }

            return true;
        }
    }
}
