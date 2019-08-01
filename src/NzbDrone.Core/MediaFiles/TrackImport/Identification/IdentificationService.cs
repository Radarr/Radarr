using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public interface IIdentificationService
    {
        List<LocalAlbumRelease> Identify(List<LocalTrack> localTracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease, bool includeExisting);
    }

    public class IdentificationService : IIdentificationService
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly ITrackService _trackService;
        private readonly ITrackGroupingService _trackGroupingService;
        private readonly IFingerprintingService _fingerprintingService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAugmentingService _augmentingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public IdentificationService(IArtistService artistService,
                                     IAlbumService albumService,
                                     IReleaseService releaseService,
                                     ITrackService trackService,
                                     ITrackGroupingService trackGroupingService,
                                     IFingerprintingService fingerprintingService,
                                     IAudioTagService audioTagService,
                                     IAugmentingService augmentingService,
                                     IMediaFileService mediaFileService,
                                     IConfigService configService,
                                     Logger logger)
        {
            _artistService = artistService;
            _albumService = albumService;
            _releaseService = releaseService;
            _trackService = trackService;
            _trackGroupingService = trackGroupingService;
            _fingerprintingService = fingerprintingService;
            _audioTagService = audioTagService;
            _augmentingService = augmentingService;
            _mediaFileService = mediaFileService;
            _configService = configService;
            _logger = logger;
        }

        private readonly List<IsoCountry> preferredCountries = new List<string> {
            "United States",
            "United Kingdom",
            "Europe",
            "[Worldwide]"
        }.Select(x => IsoCountries.Find(x)).ToList();

        private readonly List<string> VariousArtistNames = new List<string> { "various artists", "various", "va", "unknown" };
        private readonly List<string> VariousArtistIds = new List<string> { "89ad4ac3-39f7-470e-963a-56509c546377" };

        private void LogTestCaseOutput(List<LocalTrack> localTracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease)
        {
            var trackData = localTracks.Select(x => new BasicLocalTrack {
                    Path = x.Path,
                    FileTrackInfo = x.FileTrackInfo
                });
            var options = new IdTestCase {
                ExpectedMusicBrainzReleaseIds = new List<string> {"expected-id-1", "expected-id-2", "..."},
                LibraryArtists = new List<ArtistTestCase> {
                    new ArtistTestCase {
                        Artist = artist?.Metadata.Value.ForeignArtistId ?? "expected-artist-id (dev: don't forget to add metadata profile)",
                        MetadataProfile = artist?.MetadataProfile.Value
                    }
                },
                Artist = artist?.Metadata.Value.ForeignArtistId,
                Album = album?.ForeignAlbumId,
                Release = release?.ForeignReleaseId,
                NewDownload = newDownload,
                SingleRelease = singleRelease,
                Tracks = trackData.ToList()
            };

            var SerializerSettings = Json.GetSerializerSettings();
            SerializerSettings.Formatting = Formatting.None;

            var output = JsonConvert.SerializeObject(options, SerializerSettings);

            _logger.Debug($"*** IdentificationService TestCaseGenerator ***\n{output}");
        }

        public List<LocalAlbumRelease> Identify(List<LocalTrack> localTracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease, bool includeExisting)
        {
            // 1 group localTracks so that we think they represent a single release
            // 2 get candidates given specified artist, album and release.  Candidates can include extra files already on disk.
            // 3 find best candidate
            // 4 If best candidate worse than threshold, try fingerprinting

            var watch = System.Diagnostics.Stopwatch.StartNew();

            _logger.Debug("Starting track identification");
            LogTestCaseOutput(localTracks, artist, album, release, newDownload, singleRelease);

            List<LocalAlbumRelease> releases = null;
            if (singleRelease)
            {
                releases = new List<LocalAlbumRelease>{ new LocalAlbumRelease(localTracks) };
            }
            else
            {
                releases = _trackGroupingService.GroupTracks(localTracks);
            }

            _logger.Debug($"Sorted {localTracks.Count} tracks into {releases.Count} releases in {watch.ElapsedMilliseconds}ms");
            
            foreach (var localRelease in releases)
            {
                try
                {
                    _augmentingService.Augment(localRelease);
                }
                catch (AugmentingFailedException)
                {
                    _logger.Warn($"Augmentation failed for {localRelease}");
                }
                IdentifyRelease(localRelease, artist, album, release, newDownload, includeExisting);
            }

            watch.Stop();

            _logger.Debug($"Track identification for {localTracks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            return releases;
        }

        private bool FingerprintingAllowed(bool newDownload)
        {
            if (_configService.AllowFingerprinting == AllowFingerprinting.Never ||
                (_configService.AllowFingerprinting == AllowFingerprinting.NewFiles && !newDownload))
            {
                return false;
            }

            return true;
        }

        private bool ShouldFingerprint(LocalAlbumRelease localAlbumRelease)
        {
            var worstTrackMatchDist = localAlbumRelease.TrackMapping?.Mapping
                .OrderByDescending(x => x.Value.Item2.NormalizedDistance())
                .First()
                .Value.Item2.NormalizedDistance() ?? 1.0;
            
            if (localAlbumRelease.Distance.NormalizedDistance() > 0.15 ||
                localAlbumRelease.TrackMapping.LocalExtra.Any() ||
                localAlbumRelease.TrackMapping.MBExtra.Any() ||
                worstTrackMatchDist > 0.40)
            {
                return true;
            }

            return false;
        }

        private List<LocalTrack> ToLocalTrack(IEnumerable<TrackFile> trackfiles, LocalAlbumRelease localRelease)
        {
            var scanned = trackfiles.Join(localRelease.LocalTracks, t => t.Path, l => l.Path, (track, localTrack) => localTrack);
            var toScan = trackfiles.ExceptBy(t => t.Path, scanned, s => s.Path, StringComparer.InvariantCulture);
            var localTracks = scanned.Concat(toScan.Select(x => new LocalTrack {
                        Path = x.Path,
                        Size = x.Size,
                        Modified = x.Modified,
                        FileTrackInfo = _audioTagService.ReadTags(x.Path),
                        ExistingFile = true,
                        AdditionalFile = true,
                        Quality = x.Quality
                    }))
                .ToList();

            localTracks.ForEach(x => _augmentingService.Augment(x, true));

            return localTracks;
        }

        private void IdentifyRelease(LocalAlbumRelease localAlbumRelease, Artist artist, Album album, AlbumRelease release, bool newDownload, bool includeExisting)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool fingerprinted = false;
            
            var candidateReleases = GetCandidatesFromTags(localAlbumRelease, artist, album, release, includeExisting);
            if (candidateReleases.Count == 0 && FingerprintingAllowed(newDownload))
            {
                _logger.Debug("No candidates found, fingerprinting");
                _fingerprintingService.Lookup(localAlbumRelease.LocalTracks, 0.5);
                fingerprinted = true;
                candidateReleases = GetCandidatesFromFingerprint(localAlbumRelease, artist, album, release, includeExisting);
            }

            if (candidateReleases.Count == 0)
            {
                // can't find any candidates even after fingerprinting
                return;
            }

            _logger.Debug($"Got {candidateReleases.Count} candidates for {localAlbumRelease.LocalTracks.Count} tracks in {watch.ElapsedMilliseconds}ms");

            var allTracks = _trackService.GetTracksByReleases(candidateReleases.Select(x => x.AlbumRelease.Id).ToList());

            // convert all the TrackFiles that represent extra files to List<LocalTrack>
            var allLocalTracks = ToLocalTrack(candidateReleases
                                              .SelectMany(x => x.ExistingTracks)
                                              .DistinctBy(x => x.Path), localAlbumRelease);

            _logger.Debug($"Retrieved {allTracks.Count} possible tracks in {watch.ElapsedMilliseconds}ms");
            
            GetBestRelease(localAlbumRelease, candidateReleases, allTracks, allLocalTracks);

            // If result isn't great and we haven't fingerprinted, try that
            // Note that this can improve the match even if we try the same candidates
            if (!fingerprinted && FingerprintingAllowed(newDownload) && ShouldFingerprint(localAlbumRelease))
            {
                _logger.Debug($"Match not good enough, fingerprinting");
                _fingerprintingService.Lookup(localAlbumRelease.LocalTracks, 0.5);

                // Only include extra possible candidates if neither album nor release are specified
                // Will generally be specified as part of manual import
                if (album == null && release == null)
                {
                    var extraCandidates = GetCandidatesFromFingerprint(localAlbumRelease, artist, album, release, includeExisting);
                    var newCandidates = extraCandidates.ExceptBy(x => x.AlbumRelease.Id, candidateReleases, y => y.AlbumRelease.Id, EqualityComparer<int>.Default);
                    candidateReleases.AddRange(newCandidates);
                    allTracks.AddRange(_trackService.GetTracksByReleases(newCandidates.Select(x => x.AlbumRelease.Id).ToList()));
                    allLocalTracks.AddRange(ToLocalTrack(newCandidates
                                                         .SelectMany(x  => x.ExistingTracks)
                                                         .DistinctBy(x => x.Path)
                                                         .ExceptBy(x => x.Path, allLocalTracks, x => x.Path, PathEqualityComparer.Instance),
                                                         localAlbumRelease));
                }

                // fingerprint all the local files in candidates we might be matching against
                _fingerprintingService.Lookup(allLocalTracks, 0.5);
                
                GetBestRelease(localAlbumRelease, candidateReleases, allTracks, allLocalTracks);
            }

            _logger.Debug($"Best release found in {watch.ElapsedMilliseconds}ms");

            localAlbumRelease.PopulateMatch();

            _logger.Debug($"IdentifyRelease done in {watch.ElapsedMilliseconds}ms");
        }

        public List<CandidateAlbumRelease> GetCandidatesFromTags(LocalAlbumRelease localAlbumRelease, Artist artist, Album album, AlbumRelease release, bool includeExisting)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            // Generally artist, album and release are null.  But if they're not then limit candidates appropriately.
            // We've tried to make sure that tracks are all for a single release.
            
            List<CandidateAlbumRelease> candidateReleases;

            // if we have a release ID, use that
            AlbumRelease tagMbidRelease = null;
            List<CandidateAlbumRelease> tagCandidate = null;
            
            var releaseIds = localAlbumRelease.LocalTracks.Select(x => x.FileTrackInfo.ReleaseMBId).Distinct().ToList();
            if (releaseIds.Count == 1 && releaseIds[0].IsNotNullOrWhiteSpace())
            {
                _logger.Debug("Selecting release from consensus ForeignReleaseId [{0}]", releaseIds[0]);
                tagMbidRelease = _releaseService.GetReleaseByForeignReleaseId(releaseIds[0], true);
                
                if (tagMbidRelease != null)
                {
                    tagCandidate = GetCandidatesByRelease(new List<AlbumRelease> { tagMbidRelease }, includeExisting);
                }
            }

            if (release != null)
            {
                // this case overrides the release picked up from the file tags
                _logger.Debug("Release {0} [{1} tracks] was forced", release, release.TrackCount);
                candidateReleases = GetCandidatesByRelease(new List<AlbumRelease> { release }, includeExisting);
            }
            else if (album != null)
            {
                // use the release from file tags if it exists and agrees with the specified album
                if (tagMbidRelease?.AlbumId == album.Id)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetCandidatesByAlbum(localAlbumRelease, album, includeExisting);
                }
            }
            else if (artist != null)
            {
                // use the release from file tags if it exists and agrees with the specified album
                if (tagMbidRelease?.Album.Value.ArtistMetadataId == artist.ArtistMetadataId)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetCandidatesByArtist(localAlbumRelease, artist, includeExisting);
                }
            }
            else
            {
                if (tagMbidRelease != null)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetCandidates(localAlbumRelease, includeExisting);                    
                }
            }

            watch.Stop();
            _logger.Debug($"Getting candidates from tags for {localAlbumRelease.LocalTracks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            // if we haven't got any candidates then try fingerprinting
            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetCandidatesByRelease(List<AlbumRelease> releases, bool includeExisting)
        {
            // get the local tracks on disk for each album
            var albumTracks = releases.Select(x => x.AlbumId)
                .Distinct()
                .ToDictionary(id => id, id => includeExisting ? _mediaFileService.GetFilesByAlbum(id) : new List<TrackFile>());

            return releases.Select(x => new CandidateAlbumRelease {
                    AlbumRelease = x,
                    ExistingTracks = albumTracks[x.AlbumId]
                }).ToList();
        }

        private List<CandidateAlbumRelease> GetCandidatesByAlbum(LocalAlbumRelease localAlbumRelease, Album album, bool includeExisting)
        {
            // sort candidate releases by closest track count so that we stand a chance of
            // getting a perfect match early on
            return GetCandidatesByRelease(_releaseService.GetReleasesByAlbum(album.Id)
                                          .OrderBy(x => Math.Abs(localAlbumRelease.TrackCount - x.TrackCount))
                                          .ToList(), includeExisting);
        }

        private List<CandidateAlbumRelease> GetCandidatesByArtist(LocalAlbumRelease localAlbumRelease, Artist artist, bool includeExisting)
        {
            _logger.Trace("Getting candidates for {0}", artist);
            var candidateReleases = new List<CandidateAlbumRelease>();
            
            var albumTag = MostCommon(localAlbumRelease.LocalTracks.Select(x => x.FileTrackInfo.AlbumTitle)) ?? "";
            if (albumTag.IsNotNullOrWhiteSpace())
            {
                var possibleAlbums = _albumService.GetCandidates(artist.ArtistMetadataId, albumTag);
                foreach (var album in possibleAlbums)
                {
                    candidateReleases.AddRange(GetCandidatesByAlbum(localAlbumRelease, album, includeExisting));
                }
            }

            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetCandidates(LocalAlbumRelease localAlbumRelease, bool includeExisting)
        {
            // most general version, nothing has been specified.
            // get all plausible artists, then all plausible albums, then get releases for each of these.

            // check if it looks like VA.
            if (TrackGroupingService.IsVariousArtists(localAlbumRelease.LocalTracks))
            {
                throw new NotImplementedException("Various artists not supported");
            }

            var candidateReleases = new List<CandidateAlbumRelease>();
            
            var artistTag = MostCommon(localAlbumRelease.LocalTracks.Select(x => x.FileTrackInfo.ArtistTitle)) ?? "";
            if (artistTag.IsNotNullOrWhiteSpace())
            {
                var possibleArtists = _artistService.GetCandidates(artistTag);
                foreach (var artist in possibleArtists)
                {
                    candidateReleases.AddRange(GetCandidatesByArtist(localAlbumRelease, artist, includeExisting));
                }
            }

            return candidateReleases;
        }

        public List<CandidateAlbumRelease> GetCandidatesFromFingerprint(LocalAlbumRelease localAlbumRelease, Artist artist, Album album, AlbumRelease release, bool includeExisting)
        {
            var recordingIds = localAlbumRelease.LocalTracks.Where(x => x.AcoustIdResults != null).SelectMany(x => x.AcoustIdResults).ToList();
            var allReleases = _releaseService.GetReleasesByRecordingIds(recordingIds);

            // make sure releases are consistent with those selected by the user
            if (release != null)
            {
                allReleases = allReleases.Where(x => x.Id == release.Id).ToList();
            }
            else if (album != null)
            {
                allReleases = allReleases.Where(x => x.AlbumId == album.Id).ToList();
            }
            else if (artist != null)
            {
                allReleases = allReleases.Where(x => x.Album.Value.ArtistMetadataId == artist.ArtistMetadataId).ToList();
            }

            return GetCandidatesByRelease(allReleases.Select(x => new {
                        Release = x,
                        TrackCount = x.TrackCount,
                        CommonProportion = x.Tracks.Value.Select(y => y.ForeignRecordingId).Intersect(recordingIds).Count() / localAlbumRelease.TrackCount
                    })
                .Where(x => x.CommonProportion > 0.6)
                .ToList()
                .OrderBy(x => Math.Abs(x.TrackCount - localAlbumRelease.TrackCount))
                .ThenByDescending(x => x.CommonProportion)
                .Select(x => x.Release)
                .Take(10)
                .ToList(), includeExisting);
        }

        private T MostCommon<T>(IEnumerable<T> items)
        {
            return items.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
        }

        private void GetBestRelease(LocalAlbumRelease localAlbumRelease, List<CandidateAlbumRelease> candidateReleases, List<Track> dbTracks, List<LocalTrack> extraTracksOnDisk)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            _logger.Debug("Matching {0} track files against {1} candidates", localAlbumRelease.TrackCount, candidateReleases.Count);
            _logger.Trace("Processing files:\n{0}", string.Join("\n", localAlbumRelease.LocalTracks.Select(x => x.Path)));

            double bestDistance = 1.0;

            foreach (var candidateRelease in candidateReleases)
            {
                var release = candidateRelease.AlbumRelease;
                _logger.Debug("Trying Release {0} [{1}, {2} tracks, {3} existing]", release, release.Title, release.TrackCount, candidateRelease.ExistingTracks.Count);
                var rwatch = System.Diagnostics.Stopwatch.StartNew();

                var extraTrackPaths = candidateRelease.ExistingTracks.Select(x => x.Path).ToList();
                var extraTracks = extraTracksOnDisk.Where(x => extraTrackPaths.Contains(x.Path)).ToList();
                var allLocalTracks = localAlbumRelease.LocalTracks.Concat(extraTracks).DistinctBy(x => x.Path).ToList();
                
                var mapping = MapReleaseTracks(allLocalTracks, dbTracks.Where(x => x.AlbumReleaseId == release.Id).ToList());
                var distance = AlbumReleaseDistance(allLocalTracks, release, mapping);
                var currDistance = distance.NormalizedDistance();

                rwatch.Stop();
                _logger.Debug("Release {0} [{1} tracks] has distance {2} vs best distance {3} [{4}ms]",
                              release, release.TrackCount, currDistance, bestDistance, rwatch.ElapsedMilliseconds);
                if (currDistance < bestDistance)
                {
                    bestDistance = currDistance;
                    localAlbumRelease.Distance = distance;
                    localAlbumRelease.AlbumRelease = release;
                    localAlbumRelease.ExistingTracks = extraTracks;
                    localAlbumRelease.TrackMapping = mapping;
                    if (currDistance == 0.0)
                    {
                        break;
                    }
                }
            }

            watch.Stop();
            _logger.Debug($"Best release: {localAlbumRelease.AlbumRelease} Distance {localAlbumRelease.Distance.NormalizedDistance()} found in {watch.ElapsedMilliseconds}ms");
        }

        public int GetTotalTrackNumber(Track track, List<Track> allTracks)
        {
            return track.AbsoluteTrackNumber + allTracks.Count(t => t.MediumNumber < track.MediumNumber);
        }

        public TrackMapping MapReleaseTracks(List<LocalTrack> localTracks, List<Track> mbTracks)
        {
            var distances = new Distance[localTracks.Count, mbTracks.Count];
            var costs = new double[localTracks.Count, mbTracks.Count];

            for (int col = 0; col < mbTracks.Count; col++)
            {
                var totalTrackNumber = GetTotalTrackNumber(mbTracks[col], mbTracks);
                for (int row = 0; row < localTracks.Count; row++)
                {
                    distances[row, col] = TrackDistance(localTracks[row], mbTracks[col], totalTrackNumber, false);
                    costs[row, col] = distances[row, col].NormalizedDistance();
                }
            }

            var m = new Munkres(costs);
            m.Run();

            var result = new TrackMapping();
            foreach (var pair in m.Solution)
            {
                result.Mapping.Add(localTracks[pair.Item1], Tuple.Create(mbTracks[pair.Item2], distances[pair.Item1, pair.Item2]));
                _logger.Trace("Mapped {0} to {1}, dist: {2}", localTracks[pair.Item1], mbTracks[pair.Item2], costs[pair.Item1, pair.Item2]);
            }
            
            result.LocalExtra = localTracks.Except(result.Mapping.Keys).ToList();
            _logger.Trace($"Unmapped files:\n{string.Join("\n", result.LocalExtra)}");
            
            result.MBExtra = mbTracks.Except(result.Mapping.Values.Select(x => x.Item1)).ToList();
            _logger.Trace($"Missing tracks:\n{string.Join("\n", result.MBExtra)}");

            return result;
        }

        private bool TrackIndexIncorrect(LocalTrack localTrack, Track mbTrack, int totalTrackNumber)
        {
            return localTrack.FileTrackInfo.TrackNumbers[0] != mbTrack.AbsoluteTrackNumber &&
                localTrack.FileTrackInfo.TrackNumbers[0] != totalTrackNumber;
        }

        public Distance TrackDistance(LocalTrack localTrack, Track mbTrack, int totalTrackNumber, bool includeArtist = false)
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

        public Distance AlbumReleaseDistance(List<LocalTrack> localTracks, AlbumRelease release, TrackMapping mapping)
        {
            var dist = new Distance();

            if (!VariousArtistIds.Contains(release.Album.Value.ArtistMetadata.Value.ForeignArtistId))
            {
                var artist = MostCommon(localTracks.Select(x => x.FileTrackInfo.ArtistTitle)) ?? "";
                dist.AddString("artist", artist, release.Album.Value.ArtistMetadata.Value.Name);
                _logger.Trace("artist: {0} vs {1}; {2}", artist, release.Album.Value.ArtistMetadata.Value.Name, dist.NormalizedDistance());
            }

            var title = MostCommon(localTracks.Select(x => x.FileTrackInfo.AlbumTitle)) ?? "";
            // Use the album title since the differences in release titles can cause confusion and
            // aren't always correct in the tags
            dist.AddString("album", title, release.Album.Value.Title);
            _logger.Trace("album: {0} vs {1}; {2}", title, release.Title, dist.NormalizedDistance());

            // Number of discs, either as tagged or the max disc number seen
            var discCount = MostCommon(localTracks.Select(x => x.FileTrackInfo.DiscCount));
            discCount = discCount != 0 ? discCount : localTracks.Max(x => x.FileTrackInfo.DiscNumber);
            if (discCount > 0)
            {
                dist.AddNumber("media_count", discCount, release.Media.Count);
                _logger.Trace("media_count: {0} vs {1}; {2}", discCount, release.Media.Count, dist.NormalizedDistance());
            }

            // Media format
            if (release.Media.Select(x => x.Format).Contains("Unknown"))
            {
                dist.Add("media_format", 1.0);
            }

            // Year
            var localYear = MostCommon(localTracks.Select(x => x.FileTrackInfo.Year));
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
                _logger.Trace($"year: {localYear} vs {release.Album.Value.ReleaseDate?.Year} or {release.ReleaseDate?.Year}; {dist.NormalizedDistance()}");
            }

            // If we parsed a country from the files use that, otherwise use our preference
            var country = MostCommon(localTracks.Select(x => x.FileTrackInfo.Country));
            if (release.Country.Count > 0)
            {
                if (country != null)
                {
                    dist.AddEquality("country", country.Name, release.Country);
                    _logger.Trace("country: {0} vs {1}; {2}", country.Name, string.Join(", ", release.Country), dist.NormalizedDistance());
                }
                else if (preferredCountries.Count > 0)
                {
                    dist.AddPriority("country", release.Country, preferredCountries.Select(x => x.Name).ToList());
                    _logger.Trace("country priority: {0} vs {1}; {2}", string.Join(", ", preferredCountries.Select(x => x.Name)), string.Join(", ", release.Country), dist.NormalizedDistance());
                }
            }
            else
            {
                // full penalty if MusicBrainz release is missing a country
                dist.Add("country", 1.0);
            }

            var label = MostCommon(localTracks.Select(x => x.FileTrackInfo.Label));
            if (label.IsNotNullOrWhiteSpace())
            {
                dist.AddEquality("label", label, release.Label);
                _logger.Trace("label: {0} vs {1}; {2}", label, string.Join(", ", release.Label), dist.NormalizedDistance());
            }

            var disambig = MostCommon(localTracks.Select(x => x.FileTrackInfo.Disambiguation));
            if (disambig.IsNotNullOrWhiteSpace())
            {
                dist.AddString("album_disambiguation", disambig, release.Disambiguation);
                _logger.Trace("album_disambiguation: {0} vs {1}; {2}", disambig, release.Disambiguation, dist.NormalizedDistance());
            }
            
            var mbAlbumId = MostCommon(localTracks.Select(x => x.FileTrackInfo.ReleaseMBId));
            if (mbAlbumId.IsNotNullOrWhiteSpace())
            {
                dist.AddBool("album_id", mbAlbumId != release.ForeignReleaseId && !release.OldForeignReleaseIds.Contains(mbAlbumId));
                _logger.Trace("album_id: {0} vs {1} or {2}; {3}", mbAlbumId, release.ForeignReleaseId, string.Join(", ", release.OldForeignReleaseIds), dist.NormalizedDistance());
            }

            // tracks
            foreach (var pair in mapping.Mapping)
            {
                dist.Add("tracks", pair.Value.Item2.NormalizedDistance());
            }
            _logger.Trace("after trackMapping: {0}", dist.NormalizedDistance());

            // missing tracks
            foreach (var track in mapping.MBExtra.Take(localTracks.Count))
            {
                dist.Add("missing_tracks", 1.0);
            }
            _logger.Trace("after missing tracks: {0}", dist.NormalizedDistance());

            // unmatched tracks
            foreach (var track in mapping.LocalExtra.Take(localTracks.Count))
            {
                dist.Add("unmatched_tracks", 1.0);
            }
            _logger.Trace("after unmatched tracks: {0}", dist.NormalizedDistance());

            return dist;
        }
    }
}
