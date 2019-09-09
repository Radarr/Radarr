using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public interface ITrackGroupingService
    {
        List<LocalAlbumRelease> GroupTracks(List<LocalTrack> localTracks);
    }

    public class TrackGroupingService : ITrackGroupingService
    {
        private static readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(TrackGroupingService));

        private static readonly List<string> multiDiscMarkers = new List<string> { @"dis[ck]", @"cd" };
        private static readonly string multiDiscPatternFormat = @"^(?<root>.*%s[\W_]*)\d";
        private static readonly List<string> VariousArtistTitles = new List<string> { "", "various artists", "various", "va", "unknown" };

        public List<LocalAlbumRelease> GroupTracks(List<LocalTrack> localTracks)
        {
            var releases = new List<LocalAlbumRelease>();

            // first attempt, assume grouped by folder
            var unprocessed = new List<LocalTrack>();
            foreach (var group in GroupTracksByDirectory(localTracks))
            {
                var tracks = group.ToList();
                if (LooksLikeSingleRelease(tracks))
                {
                    releases.Add(new LocalAlbumRelease(tracks));
                }
                else
                {
                    unprocessed.AddRange(tracks);
                }
            }

            // If anything didn't get grouped correctly, try grouping by Album (to pick up VA)
            var unprocessed2 = new List<LocalTrack>();
            foreach (var group in unprocessed.GroupBy(x => x.FileTrackInfo.AlbumTitle))
            {
                _logger.Debug("Falling back to grouping by album tag");
                var tracks = group.ToList();
                if (LooksLikeSingleRelease(tracks))
                {
                    releases.Add(new LocalAlbumRelease(tracks));
                }
                else
                {
                    unprocessed2.AddRange(tracks);
                }
            }

            // Finally fall back to grouping by Album/Artist pair
            foreach (var group in unprocessed2.GroupBy(x => new { x.FileTrackInfo.ArtistTitle, x.FileTrackInfo.AlbumTitle} ))
            {
                _logger.Debug("Falling back to grouping by album+artist tag");
                releases.Add(new LocalAlbumRelease(group.ToList()));
            }

            return releases;
        }

        private static bool HasCommonEntry(IEnumerable<string> values, double threshold, double fuzz)
        {
            var groups = values.GroupBy(x => x).OrderByDescending(x => x.Count());
            var distinctCount = groups.Count();
            var mostCommonCount = groups.First().Count();
            var mostCommonEntry = groups.First().Key;
            var totalCount = values.Count();

            // merge groups that are close to the most common value
            foreach(var group in groups.Skip(1))
            {
                if (mostCommonEntry.IsNotNullOrWhiteSpace() &&
                    group.Key.IsNotNullOrWhiteSpace() &&
                    mostCommonEntry.LevenshteinCoefficient(group.Key) > fuzz)
                {
                    distinctCount--;
                    mostCommonCount += group.Count();
                }
            }

            _logger.Trace($"DistinctCount {distinctCount} MostCommonCount {mostCommonCount} TotalCout {totalCount}");

            if (distinctCount > 1 &&
                (distinctCount / (double)totalCount > threshold ||
                 mostCommonCount / (double) totalCount < 1 - threshold))
            {
                return false;
            }

            return true;
        }

        public static bool LooksLikeSingleRelease(List<LocalTrack> tracks)
        {
            // returns true if we think all the tracks belong to a single release

            // artist/album tags must be the same for 75% of tracks, with no more than 25% having different values
            // (except in the case of various artists)
            
            const double albumTagThreshold = 0.25;
            const double artistTagThreshold = 0.25;
            const double tagFuzz = 0.9;
            
            // check that any Album/Release MBID is unique
            if (tracks.Select(x => x.FileTrackInfo.AlbumMBId).Distinct().Where(x => x.IsNotNullOrWhiteSpace()).Count() > 1 ||
                tracks.Select(x => x.FileTrackInfo.ReleaseMBId).Distinct().Where(x => x.IsNotNullOrWhiteSpace()).Count() > 1)
            {
                _logger.Trace("LooksLikeSingleRelease: MBIDs are not unique");
                return false;
            }
            
            // check that there's a common album tag.
            var albumTags = tracks.Select(x => x.FileTrackInfo.AlbumTitle);
            if (!HasCommonEntry(albumTags, albumTagThreshold, tagFuzz))
            {
                _logger.Trace("LooksLikeSingleRelease: No common album tag");
                return false;
            }

            // If not various artists, make sure artists are sensible
            if (!IsVariousArtists(tracks))
            {
                var artistTags = tracks.Select(x => x.FileTrackInfo.ArtistTitle);
                if (!HasCommonEntry(artistTags, artistTagThreshold, tagFuzz))
                {
                    _logger.Trace("LooksLikeSingleRelease: No common artist tag");
                    return false;
                }
            }

            return true;
        }

        public static bool IsVariousArtists(List<LocalTrack> tracks)
        {
            // checks whether most common title is a known VA title
            // Also checks whether more than 75% of tracks have a distinct artist and that the most common artist
            // is responsible for < 25% of tracks
            const double artistTagThreshold = 0.75;
            const double tagFuzz = 0.9;

            var artistTags = tracks.Select(x => x.FileTrackInfo.ArtistTitle);

            if (!HasCommonEntry(artistTags, artistTagThreshold, tagFuzz))
            {
                return true;
            }

            if (VariousArtistTitles.Contains(artistTags.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private IEnumerable<List<LocalTrack>> GroupTracksByDirectory(List<LocalTrack> tracks)
        {
            // we want to check for layouts like:
            // xx/CD1/1.mp3
            // xx/CD2/1.mp3
            // or
            // yy Disc 1/1.mp3
            // yy Disc 2/1.mp3
            // and group them.

            // we only bother doing this for the immediate parent directory.
            var trackFolders = tracks.Select(x => Tuple.Create(x, Path.GetDirectoryName(x.Path)));
            
            var distinctFolders = trackFolders.Select(x => x.Item2).Distinct().ToList();
            distinctFolders.Sort();

            _logger.Trace("Folders:\n{0}", string.Join("\n", distinctFolders));

            Regex subdirRegex = null;
            var output = new List<LocalTrack>();
            foreach (var folder in distinctFolders)
            {
                if (subdirRegex != null)
                {
                    if (subdirRegex.IsMatch(folder))
                    {
                        // current folder continues match, so append output
                        output.AddRange(tracks.Where(x => x.Path.StartsWith(folder)));
                        continue;
                    }
                }

                // we have finished a multi disc match.  yield the previous output
                // and check current folder
                if (output.Count > 0)
                {
                    _logger.Trace("Yielding from 1:\n{0}", string.Join("\n", output));
                    yield return output;

                    output = new List<LocalTrack>();
                }

                // reset and put current folder into output
                subdirRegex = null;
                var currentTracks = trackFolders.Where(x => x.Item2.Equals(folder, DiskProviderBase.PathStringComparison))
                    .Select(x => x.Item1);
                output.AddRange(currentTracks);

                // check if the start of another multi disc match
                foreach (var marker in multiDiscMarkers)
                {
                    // check if this is the first of a multi-disc set of folders
                    var pattern = multiDiscPatternFormat.Replace("%s", marker);
                    var multiStartRegex = new Regex(pattern, RegexOptions.IgnoreCase);

                    var match = multiStartRegex.Match(folder);
                    if (match.Success)
                    {
                        var subdirPattern = $"^{Regex.Escape(match.Groups["root"].ToString())}\\d+$";
                        subdirRegex = new Regex(subdirPattern, RegexOptions.IgnoreCase);
                        break;
                    }
                }

                if (subdirRegex == null)
                {
                    // not the start of a multi-disc match, yield
                    _logger.Trace("Yielding from 2:\n{0}", string.Join("\n", output));
                    yield return output;

                    // reset output
                    output = new List<LocalTrack>();
                }
            }

            // return the final stored output
            if (output.Count > 0)
            {
                _logger.Trace("Yielding final:\n{0}", string.Join("\n", output));
                yield return output;
            }
        }
    }
}
