namespace NzbDrone.Core.Parser
{
    public static class SceneChecker
    {
        //This method should prefer false negatives over false positives.
        //It's better not to use a title that might be scene than to use one that isn't scene
        public static string GetSceneTitle(string title)
        {
            if (title == null)
            {
                return null;
            }

            if (!title.Contains("."))
            {
                return null;
            }

            if (title.Contains(" "))
            {
                return null;
            }

            var parsedTitle = Parser.ParseMovieTitle(title);

            if (parsedTitle == null ||
                parsedTitle.ReleaseGroup == null ||
                parsedTitle.Quality.Quality == Qualities.Quality.Unknown ||
                string.IsNullOrWhiteSpace(parsedTitle.PrimaryMovieTitle) ||
                string.IsNullOrWhiteSpace(parsedTitle.ReleaseTitle))
            {
                return null;
            }

            return parsedTitle.ReleaseTitle;
        }

        public static bool IsSceneTitle(string title)
        {
            return GetSceneTitle(title) != null;
        }
    }
}
