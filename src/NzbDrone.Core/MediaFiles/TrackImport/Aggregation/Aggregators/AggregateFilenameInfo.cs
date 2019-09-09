using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators
{
    public class AggregateFilenameInfo : IAggregate<LocalAlbumRelease>
    {
        private readonly Logger _logger;

        private static readonly List<Tuple<string, string>> charsAndSeps = new List<Tuple<string, string>> {
            Tuple.Create(@"a-z0-9,\(\)\.&'’\s", @"\s_-"),
            Tuple.Create(@"a-z0-9,\(\)\.\&'’_", @"\s-")
        };

        private static Regex[] Patterns(string chars, string sep)
        {
            var sep1 = $@"(?<sep>[{sep}]+)";
            var sepn = @"\k<sep>";
            var artist = $@"(?<artist>[{chars}]+)";
            var track = $@"(?<track>\d+)";
            var title = $@"(?<title>[{chars}]+)";
            var tag = $@"(?<tag>[{chars}]+)";
            
            return new [] {
                new Regex($@"^{track}{sep1}{artist}{sepn}{title}{sepn}{tag}$", RegexOptions.IgnoreCase),
                new Regex($@"^{track}{sep1}{artist}{sepn}{tag}{sepn}{title}$", RegexOptions.IgnoreCase),
                new Regex($@"^{track}{sep1}{artist}{sepn}{title}$", RegexOptions.IgnoreCase),
                
                new Regex($@"^{artist}{sep1}{tag}{sepn}{track}{sepn}{title}$", RegexOptions.IgnoreCase),
                new Regex($@"^{artist}{sep1}{track}{sepn}{title}{sepn}{tag}$", RegexOptions.IgnoreCase),
                new Regex($@"^{artist}{sep1}{track}{sepn}{title}$", RegexOptions.IgnoreCase),
                
                new Regex($@"^{artist}{sep1}{title}{sepn}{tag}$", RegexOptions.IgnoreCase),
                new Regex($@"^{artist}{sep1}{tag}{sepn}{title}$", RegexOptions.IgnoreCase),
                new Regex($@"^{artist}{sep1}{title}$", RegexOptions.IgnoreCase),

                new Regex($@"^{track}{sep1}{title}$", RegexOptions.IgnoreCase),
                new Regex($@"^{track}{sep1}{tag}{sepn}{title}$", RegexOptions.IgnoreCase),
                new Regex($@"^{track}{sep1}{title}{sepn}{tag}$", RegexOptions.IgnoreCase),
                
                new Regex($@"^{title}$", RegexOptions.IgnoreCase),
            };
        }

        public AggregateFilenameInfo(Logger logger)
        {
            _logger = logger;
        }
        
        public LocalAlbumRelease Aggregate(LocalAlbumRelease release, bool others)
        {
            var tracks = release.LocalTracks;
            if (tracks.Count(x => x.FileTrackInfo.Title.IsNullOrWhiteSpace()) > 0
                || tracks.Count(x => x.FileTrackInfo.TrackNumbers.First() == 0) > 0
                || tracks.Count(x => x.FileTrackInfo.DiscNumber == 0) > 0)
            {
                _logger.Debug("Missing data in tags, trying filename augmentation");
                foreach (var charSep in charsAndSeps)
                {
                    foreach (var pattern in Patterns(charSep.Item1, charSep.Item2))
                    {
                        var matches = AllMatches(tracks, pattern);
                        if (matches != null)
                        {
                            ApplyMatches(matches, pattern);
                        }
                    }
                }
            }

            return release;
        }

        private Dictionary<LocalTrack, Match> AllMatches(List<LocalTrack> tracks, Regex pattern)
        {
            var matches = new Dictionary<LocalTrack, Match>();
            foreach (var track in tracks)
            {
                var filename = Path.GetFileNameWithoutExtension(track.Path).RemoveAccent();
                var match = pattern.Match(filename);
                _logger.Trace("Matching '{0}' against regex {1}", filename, pattern);
                if (match.Success && match.Groups[0].Success)
                {
                    matches[track] = match;
                }
                else
                {
                    return null;
                }
            }
            return matches;
        }

        private bool EqualFields(IEnumerable<Match> matches, string field)
        {
            return matches.Select(x => x.Groups[field].Value).Distinct().Count() == 1;
        }

        private void ApplyMatches(Dictionary<LocalTrack, Match> matches, Regex pattern)
        {
            _logger.Debug("Got filename match with regex {0}", pattern);
            
            var keys = pattern.GetGroupNames();
            var someMatch = matches.First().Value;

            // only proceed if the 'tag' field is equal across all filenames
            if (keys.Contains("tag") && !EqualFields(matches.Values, "tag"))
            {
                _logger.Trace("Abort - 'tag' varies between matches");
                return;
            }

            // Given both an "artist" and "title" field, assume that one is
            // *actually* the artist, which must be uniform, and use the other
            // for the title. This, of course, won't work for VA albums.
            string titleField;
            string artist;
            if (keys.Contains("artist"))
            {
                if (EqualFields(matches.Values, "artist"))
                {
                    artist = someMatch.Groups["artist"].Value.Trim();
                    titleField = "title";
                }
                else if (EqualFields(matches.Values, "title"))
                {
                    artist = someMatch.Groups["title"].Value.Trim();
                    titleField = "artist";
                }
                else
                {
                    _logger.Trace("Abort - both artist and title vary between matches");
                    // both vary, abort
                    return;
                }

                _logger.Debug("Got artist from filename: {0}", artist);

                foreach (var track in matches.Keys)
                {
                    if (track.FileTrackInfo.ArtistTitle.IsNullOrWhiteSpace())
                    {
                        track.FileTrackInfo.ArtistTitle = artist;
                    }
                }
            }
            else
            {
                // no artist - remaining field is the title
                titleField = "title";
            }

            // Apply the title and track
            foreach (var track in matches.Keys)
            {
                if (track.FileTrackInfo.Title.IsNullOrWhiteSpace())
                {
                    var title = matches[track].Groups[titleField].Value.Trim();
                    _logger.Debug("Got title from filename: {0}", title);
                    track.FileTrackInfo.Title = title;
                }

                var trackNums = track.FileTrackInfo.TrackNumbers;
                if (keys.Contains("track") && (trackNums.Count() == 0 || trackNums.First() == 0))
                {
                    var tracknum = Convert.ToInt32(matches[track].Groups["track"].Value);
                    if (tracknum > 100)
                    {
                        track.FileTrackInfo.DiscNumber = tracknum / 100;
                        _logger.Debug("Got disc number from filename: {0}", tracknum / 100);
                        tracknum = tracknum % 100;
                    }
                    _logger.Debug("Got track number from filename: {0}", tracknum);
                    track.FileTrackInfo.TrackNumbers = new [] { tracknum };
                }
            }
        }
    }
}
