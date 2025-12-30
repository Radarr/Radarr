using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedEpisodeInfo
    {
        public ParsedEpisodeInfo()
        {
            Languages = new List<Language>();
            EpisodeNumbers = Array.Empty<int>();
            AbsoluteEpisodeNumbers = Array.Empty<int>();
        }

        public string SeriesTitle { get; set; }
        public string OriginalTitle { get; set; }
        public string ReleaseTitle { get; set; }
        public string SimpleReleaseTitle { get; set; }
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }

        public int SeasonNumber { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public string AirDate { get; set; }

        public bool FullSeason { get; set; }
        public bool IsPartialSeason { get; set; }
        public bool IsMultiSeason { get; set; }
        public bool IsSeasonExtra { get; set; }
        public bool IsSplitEpisode { get; set; }
        public bool IsDaily { get; set; }
        public bool IsAbsoluteNumbering { get; set; }
        public bool IsPossibleSpecialEpisode { get; set; }

        public int? ReleaseVersion { get; set; }
        public StreamingSource StreamingSource { get; set; }

        public bool IsPossibleSceneSeasonSpecial => SeasonNumber != 0 &&
                                                     (ReleaseTitle?.Contains("Special") == true ||
                                                      ReleaseTitle?.Contains("Specials") == true);

        public bool IsSpecialEpisode => SeasonNumber == 0 ||
                                        EpisodeNumbers?.Any(e => e == 0) == true ||
                                        IsPossibleSpecialEpisode;

        public override string ToString()
        {
            var episodeNumbers = EpisodeNumbers?.Any() == true
                ? string.Format("E{0}", string.Join("-", EpisodeNumbers.Select(e => e.ToString("D2"))))
                : string.Empty;

            var absoluteNumbers = AbsoluteEpisodeNumbers?.Any() == true
                ? string.Format(" ({0})", string.Join("-", AbsoluteEpisodeNumbers))
                : string.Empty;

            if (IsDaily)
            {
                return string.Format("{0} - {1} {2}", SeriesTitle, AirDate, Quality);
            }

            return string.Format("{0} - S{1:D2}{2}{3} {4}",
                SeriesTitle,
                SeasonNumber,
                episodeNumbers,
                absoluteNumbers,
                Quality);
        }
    }
}
