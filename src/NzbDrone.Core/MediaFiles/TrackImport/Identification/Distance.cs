using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public class Distance
    {
        private Dictionary<string, List<double>> penalties;

        // from beets default config
        private static readonly Dictionary<string, double> weights = new Dictionary<string, double>
        {
            { "source", 2.0 },
            { "artist", 3.0 },
            { "album", 3.0 },
            { "media_count", 1.0 },
            { "media_format", 1.0 },
            { "year", 1.0 },
            { "country", 0.5 },
            { "label", 0.5 },
            { "catalog_number", 0.5 },
            { "album_disambiguation", 0.5 },
            { "album_id", 5.0 },
            { "tracks", 2.0 },
            { "missing_tracks", 0.6 },
            { "unmatched_tracks", 0.9 },
            { "track_title", 3.0 },
            { "track_artist", 2.0 },
            { "track_index", 1.0 },
            { "track_length", 2.0 },
            { "recording_id", 10.0 },
        };

        public Distance()
        {
            penalties = new Dictionary<string, List<double>>(15);
        }

        public Dictionary<string, List<double>> Penalties => penalties;
        public string Reasons => penalties.Count(x => x.Value.Max() > 0.0) > 0 ? "[" + string.Join(", ", Penalties.Where(x => x.Value.Max() > 0.0).Select(x => x.Key.Replace('_', ' '))) + "]" : string.Empty;

        private double MaxDistance(Dictionary<string, List<double>> penalties)
        {
            return penalties.Select(x => x.Value.Count * weights[x.Key]).Sum();
        }

        public double MaxDistance()
        {
            return MaxDistance(penalties);
        }

        private double RawDistance(Dictionary<string, List<double>> penalties)
        {
            return penalties.Select(x => x.Value.Sum() * weights[x.Key]).Sum();
        }
        
        public double RawDistance()
        {
            return RawDistance(penalties);
        }

        private double NormalizedDistance(Dictionary<string, List<double>> penalties)
        {
            var max = MaxDistance(penalties);
            return max > 0 ? RawDistance(penalties) / max : 0;
        }

        public double NormalizedDistance()
        {
            return NormalizedDistance(penalties);
        }

        public double NormalizedDistanceExcluding(List<string> keys)
        {
            return NormalizedDistance(penalties.Where(x => !keys.Contains(x.Key)).ToDictionary(y => y.Key, y => y.Value));
        }

        public void Add(string key, double dist)
        {
            if (penalties.ContainsKey(key))
            {
                penalties[key].Add(dist);                    
            }
            else
            {
                penalties[key] = new List<double> { dist };
            }
        }

        public void AddRatio(string key, double value, double target)
        {
            // Adds a distance penalty for value as a ratio of target
            // value is between 0 and target
            var dist = target > 0 ? Math.Max(Math.Min(value, target), 0.0) / target : 0.0;
            Add(key, dist);
        }

        public void AddNumber(string key, int value, int target)
        {
            var diff = Math.Abs(value - target);
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    Add(key, 1.0);
                }
            }
            else
            {
                Add(key, 0.0);
            }
        }

        private static string Clean(string input)
        {
            char[] arr = input.ToLower().RemoveAccent().ToCharArray();

            arr = Array.FindAll<char>(arr, c => (char.IsLetterOrDigit(c)));

            return new string(arr);
        }

        public void AddString(string key, string value, string target)
        {
            // Adds a penaltly based on the distance between value and target
            var cleanValue = Clean(value);
            var cleanTarget = Clean(target);
            
            if (cleanValue.IsNullOrWhiteSpace() && cleanTarget.IsNotNullOrWhiteSpace())
            {
                Add(key, 1.0);
            }
            else if (cleanValue.IsNullOrWhiteSpace() && cleanTarget.IsNullOrWhiteSpace())
            {
                Add(key, 0.0);
            }
            else
            {
                Add(key, 1.0 - cleanValue.LevenshteinCoefficient(cleanTarget));
            }
        }

        public void AddBool(string key, bool expr)
        {
            Add(key, expr ? 1.0 : 0.0);
        }

        public void AddEquality<T>(string key, T value, List<T> options) where T : IEquatable<T>
        {
            Add(key, options.Contains(value) ? 0.0 : 1.0);
        }

        public void AddPriority<T>(string key, T value, List<T> options) where T : IEquatable<T>
        {
            var unit = 1.0 / (options.Count > 0 ? (double) options.Count : 1.0);
            var index = options.IndexOf(value);
            if (index == -1)
            {
                Add(key, 1.0);
            }
            else
            {
                Add(key, index * unit);
            }
        }

        public void AddPriority<T>(string key, List<T> values, List<T> options) where T : IEquatable<T>
        {
            for(int i = 0; i < options.Count; i++)
            {
                if (values.Contains(options[i]))
                {
                    Add(key, i / (double)options.Count);
                    return;
                }
            }

            Add(key, 1.0);
        }
    }
}
