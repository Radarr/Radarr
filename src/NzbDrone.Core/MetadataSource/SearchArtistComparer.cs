using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public class SearchArtistComparer : IComparer<Artist>
    {
        private static readonly Regex RegexCleanPunctuation = new Regex("[-._:]", RegexOptions.Compiled);
        private static readonly Regex RegexCleanCountryYearPostfix = new Regex(@"(?<=.+)( \([A-Z]{2}\)| \(\d{4}\)| \([A-Z]{2}\) \(\d{4}\))$", RegexOptions.Compiled);
        private static readonly Regex ArticleRegex = new Regex(@"^(a|an|the)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string SearchQuery { get; private set; }

        private readonly string _searchQueryWithoutYear;
        private int? _year;

        public SearchArtistComparer(string searchQuery)
        {
            SearchQuery = searchQuery;
            
            var match = Regex.Match(SearchQuery, @"^(?<query>.+)\s+(?:\((?<year>\d{4})\)|(?<year>\d{4}))$");
            if (match.Success)
            {
                _searchQueryWithoutYear = match.Groups["query"].Value.ToLowerInvariant();
                _year = int.Parse(match.Groups["year"].Value);
            }
            else
            {
                _searchQueryWithoutYear = searchQuery.ToLowerInvariant();
            }
        }

        public int Compare(Artist x, Artist y)
        {
            int result = 0;

            // Prefer exact matches
            result = Compare(x, y, s => CleanPunctuation(s.Name).Equals(CleanPunctuation(SearchQuery)));
            if (result != 0) return -result;

            // Remove Articles (a/an/the)
            result = Compare(x, y, s => CleanArticles(s.Name).Equals(CleanArticles(SearchQuery)));
            if (result != 0) return -result;

            // Prefer close matches
            result = Compare(x, y, s => CleanPunctuation(s.Name).LevenshteinDistance(CleanPunctuation(SearchQuery)) <= 1);
            if (result != 0) return -result;
         
            return Compare(x, y, s => SearchQuery.LevenshteinDistanceClean(s.Name));
        }
        
        public int Compare<T>(Artist x, Artist y, Func<Artist, T> keySelector)
            where T : IComparable<T>
        {
            var keyX = keySelector(x);
            var keyY = keySelector(y);

            return keyX.CompareTo(keyY);
        }

        private string CleanPunctuation(string title)
        {
            title = RegexCleanPunctuation.Replace(title, "");

            return title.ToLowerInvariant();
        }

        private string CleanTitle(string title)
        {
            title = RegexCleanPunctuation.Replace(title, "");
            title = RegexCleanCountryYearPostfix.Replace(title, "");

            return title.ToLowerInvariant();
        }

        private string CleanArticles(string title)
        {
            title = ArticleRegex.Replace(title, "");

            return title.Trim().ToLowerInvariant();
        }

    }
}
