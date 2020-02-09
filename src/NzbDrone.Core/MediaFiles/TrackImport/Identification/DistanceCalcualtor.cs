using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public static class DistanceCalculator
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DistanceCalculator));

        public static readonly List<string> VariousArtistIds = new List<string> { "89ad4ac3-39f7-470e-963a-56509c546377" };
        private static readonly List<string> VariousArtistNames = new List<string> { "various artists", "various", "va", "unknown" };
        private static readonly List<IsoCountry> PreferredCountries = new List<string>
        {
            "United States",
            "United Kingdom",
            "Europe",
            "[Worldwide]"
        }.Select(x => IsoCountries.Find(x)).ToList();

        private static bool TrackIndexIncorrect(LocalTrack localTrack, Track mbTrack, int totalTrackNumber)
        {
            return localTrack.FileTrackInfo.TrackNumbers[0] != mbTrack.AbsoluteTrackNumber &&
                localTrack.FileTrackInfo.TrackNumbers[0] != totalTrackNumber;
        }

        public static int GetTotalTrackNumber(Track track, List<Track> allTracks)
        {
            return track.AbsoluteTrackNumber + allTracks.Count(t => t.MediumNumber < track.MediumNumber);
        }

        public static Distance TrackDistance(LocalTrack localTrack, Track mbTrack, int totalTrackNumber, bool includeArtist = false)
        {
            var dist = new Distance();

            var localLength = localTrack.FileTrackInfo.Duration.TotalSeconds;
            var mbLength = mbTrack.Duration / 1000;
            var diff = Math.Abs(localLength - mbLength) - 10;

            if (mbLength > 0)
            {
                dist.AddRatio("track_length", diff, 30);
            }

            // musicbrainz never has 'featuring' in the track title
            // see https://musicbrainz.org/doc/Style/Artist_Credits
            dist.AddString("track_title", localTrack.FileTrackInfo.CleanTitle ?? "", mbTrack.Title);

            if (includeArtist && localTrack.FileTrackInfo.ArtistTitle.IsNotNullOrWhiteSpace()
                && !VariousArtistNames.Any(x => x.Equals(localTrack.FileTrackInfo.ArtistTitle, StringComparison.InvariantCultureIgnoreCase)))
            {
                dist.AddString("track_artist", localTrack.FileTrackInfo.ArtistTitle, mbTrack.ArtistMetadata.Value.Name);
            }

            if (localTrack.FileTrackInfo.TrackNumbers.FirstOrDefault() > 0 && mbTrack.AbsoluteTrackNumber > 0)
            {
                dist.AddBool("track_index", TrackIndexIncorrect(localTrack, mbTrack, totalTrackNumber));
            }

            var recordingId = localTrack.FileTrackInfo.RecordingMBId;
            if (recordingId.IsNotNullOrWhiteSpace())
            {
                dist.AddBool("recording_id", localTrack.FileTrackInfo.RecordingMBId != mbTrack.ForeignRecordingId &&
                             !mbTrack.OldForeignRecordingIds.Contains(localTrack.FileTrackInfo.RecordingMBId));
            }

            // for fingerprinted files
            if (localTrack.AcoustIdResults != null)
            {
                dist.AddBool("recording_id", !localTrack.AcoustIdResults.Contains(mbTrack.ForeignRecordingId));
            }

            return dist;
        }

        public static Distance AlbumReleaseDistance(List<LocalTrack> localTracks, AlbumRelease release, TrackMapping mapping)
        {
            var dist = new Distance();

            if (!VariousArtistIds.Contains(release.Album.Value.ArtistMetadata.Value.ForeignArtistId))
            {
                var artist = localTracks.MostCommon(x => x.FileTrackInfo.ArtistTitle) ?? "";
                dist.AddString("artist", artist, release.Album.Value.ArtistMetadata.Value.Name);
                Logger.Trace("artist: {0} vs {1}; {2}", artist, release.Album.Value.ArtistMetadata.Value.Name, dist.NormalizedDistance());
            }

            var title = localTracks.MostCommon(x => x.FileTrackInfo.AlbumTitle) ?? "";

            // Use the album title since the differences in release titles can cause confusion and
            // aren't always correct in the tags
            dist.AddString("album", title, release.Album.Value.Title);
            Logger.Trace("album: {0} vs {1}; {2}", title, release.Title, dist.NormalizedDistance());

            // Number of discs, either as tagged or the max disc number seen
            var discCount = localTracks.MostCommon(x => x.FileTrackInfo.DiscCount);
            discCount = discCount != 0 ? discCount : localTracks.Max(x => x.FileTrackInfo.DiscNumber);
            if (discCount > 0)
            {
                dist.AddNumber("media_count", discCount, release.Media.Count);
                Logger.Trace("media_count: {0} vs {1}; {2}", discCount, release.Media.Count, dist.NormalizedDistance());
            }

            // Media format
            if (release.Media.Select(x => x.Format).Contains("Unknown"))
            {
                dist.Add("media_format", 1.0);
            }

            // Year
            var localYear = localTracks.MostCommon(x => x.FileTrackInfo.Year);
            if (localYear > 0 && (release.Album.Value.ReleaseDate.HasValue || release.ReleaseDate.HasValue))
            {
                var albumYear = release.Album.Value.ReleaseDate?.Year ?? 0;
                var releaseYear = release.ReleaseDate?.Year ?? 0;
                if (localYear == albumYear || localYear == releaseYear)
                {
                    dist.Add("year", 0.0);
                }
                else
                {
                    var remoteYear = albumYear > 0 ? albumYear : releaseYear;
                    var diff = Math.Abs(localYear - remoteYear);
                    var diff_max = Math.Abs(DateTime.Now.Year - remoteYear);
                    dist.AddRatio("year", diff, diff_max);
                }

                Logger.Trace($"year: {localYear} vs {release.Album.Value.ReleaseDate?.Year} or {release.ReleaseDate?.Year}; {dist.NormalizedDistance()}");
            }

            // If we parsed a country from the files use that, otherwise use our preference
            var country = localTracks.MostCommon(x => x.FileTrackInfo.Country);
            if (release.Country.Count > 0)
            {
                if (country != null)
                {
                    dist.AddEquality("country", country.Name, release.Country);
                    Logger.Trace("country: {0} vs {1}; {2}", country.Name, string.Join(", ", release.Country), dist.NormalizedDistance());
                }
                else if (PreferredCountries.Count > 0)
                {
                    dist.AddPriority("country", release.Country, PreferredCountries.Select(x => x.Name).ToList());
                    Logger.Trace("country priority: {0} vs {1}; {2}", string.Join(", ", PreferredCountries.Select(x => x.Name)), string.Join(", ", release.Country), dist.NormalizedDistance());
                }
            }
            else
            {
                // full penalty if MusicBrainz release is missing a country
                dist.Add("country", 1.0);
            }

            var label = localTracks.MostCommon(x => x.FileTrackInfo.Label);
            if (label.IsNotNullOrWhiteSpace())
            {
                dist.AddEquality("label", label, release.Label);
                Logger.Trace("label: {0} vs {1}; {2}", label, string.Join(", ", release.Label), dist.NormalizedDistance());
            }

            var disambig = localTracks.MostCommon(x => x.FileTrackInfo.Disambiguation);
            if (disambig.IsNotNullOrWhiteSpace())
            {
                dist.AddString("album_disambiguation", disambig, release.Disambiguation);
                Logger.Trace("album_disambiguation: {0} vs {1}; {2}", disambig, release.Disambiguation, dist.NormalizedDistance());
            }

            var mbAlbumId = localTracks.MostCommon(x => x.FileTrackInfo.ReleaseMBId);
            if (mbAlbumId.IsNotNullOrWhiteSpace())
            {
                dist.AddBool("album_id", mbAlbumId != release.ForeignReleaseId && !release.OldForeignReleaseIds.Contains(mbAlbumId));
                Logger.Trace("album_id: {0} vs {1} or {2}; {3}", mbAlbumId, release.ForeignReleaseId, string.Join(", ", release.OldForeignReleaseIds), dist.NormalizedDistance());
            }

            // tracks
            foreach (var pair in mapping.Mapping)
            {
                dist.Add("tracks", pair.Value.Item2.NormalizedDistance());
            }

            Logger.Trace("after trackMapping: {0}", dist.NormalizedDistance());

            // missing tracks
            foreach (var track in mapping.MBExtra.Take(localTracks.Count))
            {
                dist.Add("missing_tracks", 1.0);
            }

            Logger.Trace("after missing tracks: {0}", dist.NormalizedDistance());

            // unmatched tracks
            foreach (var track in mapping.LocalExtra.Take(localTracks.Count))
            {
                dist.Add("unmatched_tracks", 1.0);
            }

            Logger.Trace("after unmatched tracks: {0}", dist.NormalizedDistance());

            return dist;
        }
    }
}
