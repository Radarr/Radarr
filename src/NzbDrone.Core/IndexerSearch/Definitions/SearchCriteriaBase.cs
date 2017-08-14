using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex SpecialCharacter = new Regex(@"[`'.]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NonWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public Series Series { get; set; }
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public List<string> SceneTitles { get; set; }
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public List<Episode> Episodes { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }
        public virtual bool UserInvokedSearch { get; set; }

        public Artist Artist { get; set; }
        public List<Album> Albums { get; set; }
        public List<Track> Tracks { get; set; }

        public List<string> QueryTitles => SceneTitles.Select(GetQueryTitle).ToList();

        public static string GetQueryTitle(string title)
        {
            Ensure.That(title,() => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, string.Empty);

            cleanTitle = cleanTitle.Replace("&", "and");
            cleanTitle = SpecialCharacter.Replace(cleanTitle, "");
            cleanTitle = NonWord.Replace(cleanTitle, "+");

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            cleanTitle = cleanTitle.RemoveAccent();
            return cleanTitle.Trim('+', ' ');
        }
    }
}