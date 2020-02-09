using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
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
        List<LocalAlbumRelease> Identify(List<LocalTrack> localTracks, IdentificationOverrides idOverrides, ImportDecisionMakerConfig config);
    }

    public class IdentificationService : IIdentificationService
    {
        private readonly ITrackService _trackService;
        private readonly ITrackGroupingService _trackGroupingService;
        private readonly IFingerprintingService _fingerprintingService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAugmentingService _augmentingService;
        private readonly ICandidateService _candidateService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public IdentificationService(ITrackService trackService,
                                     ITrackGroupingService trackGroupingService,
                                     IFingerprintingService fingerprintingService,
                                     IAudioTagService audioTagService,
                                     IAugmentingService augmentingService,
                                     ICandidateService candidateService,
                                     IConfigService configService,
                                     Logger logger)
        {
            _trackService = trackService;
            _trackGroupingService = trackGroupingService;
            _fingerprintingService = fingerprintingService;
            _audioTagService = audioTagService;
            _augmentingService = augmentingService;
            _candidateService = candidateService;
            _configService = configService;
            _logger = logger;
        }

        private void LogTestCaseOutput(List<LocalTrack> localTracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease)
        {
            var trackData = localTracks.Select(x => new BasicLocalTrack
            {
                Path = x.Path,
                FileTrackInfo = x.FileTrackInfo
            });
            var options = new IdTestCase
            {
                ExpectedMusicBrainzReleaseIds = new List<string> { "expected-id-1", "expected-id-2", "..." },
                LibraryArtists = new List<ArtistTestCase>
                {
                    new ArtistTestCase
                    {
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

            var serializerSettings = Json.GetSerializerSettings();
            serializerSettings.Formatting = Formatting.None;

            var output = JsonConvert.SerializeObject(options, serializerSettings);

            _logger.Debug($"*** IdentificationService TestCaseGenerator ***\n{output}");
        }

        public List<LocalAlbumRelease> GetLocalAlbumReleases(List<LocalTrack> localTracks, bool singleRelease)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<LocalAlbumRelease> releases = null;
            if (singleRelease)
            {
                releases = new List<LocalAlbumRelease> { new LocalAlbumRelease(localTracks) };
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
            }

            return releases;
        }

        public List<LocalAlbumRelease> Identify(List<LocalTrack> localTracks, IdentificationOverrides idOverrides, ImportDecisionMakerConfig config)
        {
            // 1 group localTracks so that we think they represent a single release
            // 2 get candidates given specified artist, album and release.  Candidates can include extra files already on disk.
            // 3 find best candidate
            // 4 If best candidate worse than threshold, try fingerprinting
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _logger.Debug("Starting track identification");

            var releases = GetLocalAlbumReleases(localTracks, config.SingleRelease);

            int i = 0;
            foreach (var localRelease in releases)
            {
                i++;
                _logger.ProgressInfo($"Identifying album {i}/{releases.Count}");
                IdentifyRelease(localRelease, idOverrides, config);
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
            var localTracks = scanned.Concat(toScan.Select(x => new LocalTrack
            {
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

        private void IdentifyRelease(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, ImportDecisionMakerConfig config)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool fingerprinted = false;

            var candidateReleases = _candidateService.GetDbCandidatesFromTags(localAlbumRelease, idOverrides, config.IncludeExisting);

            if (candidateReleases.Count == 0 && config.AddNewArtists)
            {
                candidateReleases = _candidateService.GetRemoteCandidates(localAlbumRelease);
            }

            if (candidateReleases.Count == 0 && FingerprintingAllowed(config.NewDownload))
            {
                _logger.Debug("No candidates found, fingerprinting");
                _fingerprintingService.Lookup(localAlbumRelease.LocalTracks, 0.5);
                fingerprinted = true;
                candidateReleases = _candidateService.GetDbCandidatesFromFingerprint(localAlbumRelease, idOverrides, config.IncludeExisting);

                if (candidateReleases.Count == 0 && config.AddNewArtists)
                {
                    // Now fingerprints are populated this will return a different answer
                    candidateReleases = _candidateService.GetRemoteCandidates(localAlbumRelease);
                }
            }

            if (candidateReleases.Count == 0)
            {
                // can't find any candidates even after fingerprinting
                return;
            }

            _logger.Debug($"Got {candidateReleases.Count} candidates for {localAlbumRelease.LocalTracks.Count} tracks in {watch.ElapsedMilliseconds}ms");

            PopulateTracks(candidateReleases);

            // convert all the TrackFiles that represent extra files to List<LocalTrack>
            var allLocalTracks = ToLocalTrack(candidateReleases
                                              .SelectMany(x => x.ExistingTracks)
                                              .DistinctBy(x => x.Path), localAlbumRelease);

            _logger.Debug($"Retrieved {allLocalTracks.Count} possible tracks in {watch.ElapsedMilliseconds}ms");

            GetBestRelease(localAlbumRelease, candidateReleases, allLocalTracks);

            // If result isn't great and we haven't fingerprinted, try that
            // Note that this can improve the match even if we try the same candidates
            if (!fingerprinted && FingerprintingAllowed(config.NewDownload) && ShouldFingerprint(localAlbumRelease))
            {
                _logger.Debug($"Match not good enough, fingerprinting");
                _fingerprintingService.Lookup(localAlbumRelease.LocalTracks, 0.5);

                // Only include extra possible candidates if neither album nor release are specified
                // Will generally be specified as part of manual import
                if (idOverrides?.Album == null && idOverrides?.AlbumRelease == null)
                {
                    var dbCandidates = _candidateService.GetDbCandidatesFromFingerprint(localAlbumRelease, idOverrides, config.IncludeExisting);
                    var remoteCandidates = config.AddNewArtists ? _candidateService.GetRemoteCandidates(localAlbumRelease) : new List<CandidateAlbumRelease>();
                    var extraCandidates = dbCandidates.Concat(remoteCandidates);
                    var newCandidates = extraCandidates.ExceptBy(x => x.AlbumRelease.Id, candidateReleases, y => y.AlbumRelease.Id, EqualityComparer<int>.Default);
                    candidateReleases.AddRange(newCandidates);

                    PopulateTracks(candidateReleases);

                    allLocalTracks.AddRange(ToLocalTrack(newCandidates
                                                         .SelectMany(x => x.ExistingTracks)
                                                         .DistinctBy(x => x.Path)
                                                         .ExceptBy(x => x.Path, allLocalTracks, x => x.Path, PathEqualityComparer.Instance),
                                                         localAlbumRelease));
                }

                // fingerprint all the local files in candidates we might be matching against
                _fingerprintingService.Lookup(allLocalTracks, 0.5);

                GetBestRelease(localAlbumRelease, candidateReleases, allLocalTracks);
            }

            _logger.Debug($"Best release found in {watch.ElapsedMilliseconds}ms");

            localAlbumRelease.PopulateMatch();

            _logger.Debug($"IdentifyRelease done in {watch.ElapsedMilliseconds}ms");
        }

        public void PopulateTracks(List<CandidateAlbumRelease> candidateReleases)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var releasesMissingTracks = candidateReleases.Where(x => !x.AlbumRelease.Tracks.IsLoaded);
            var allTracks = _trackService.GetTracksByReleases(releasesMissingTracks.Select(x => x.AlbumRelease.Id).ToList());

            _logger.Debug($"Retrieved {allTracks.Count} possible tracks in {watch.ElapsedMilliseconds}ms");

            foreach (var release in releasesMissingTracks)
            {
                release.AlbumRelease.Tracks = allTracks.Where(x => x.AlbumReleaseId == release.AlbumRelease.Id).ToList();
            }
        }

        private void GetBestRelease(LocalAlbumRelease localAlbumRelease, List<CandidateAlbumRelease> candidateReleases, List<LocalTrack> extraTracksOnDisk)
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

                var mapping = MapReleaseTracks(allLocalTracks, release.Tracks.Value);
                var distance = DistanceCalculator.AlbumReleaseDistance(allLocalTracks, release, mapping);
                var currDistance = distance.NormalizedDistance();

                rwatch.Stop();
                _logger.Debug("Release {0} [{1} tracks] has distance {2} vs best distance {3} [{4}ms]",
                              release,
                              release.TrackCount,
                              currDistance,
                              bestDistance,
                              rwatch.ElapsedMilliseconds);
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

        public TrackMapping MapReleaseTracks(List<LocalTrack> localTracks, List<Track> mbTracks)
        {
            var distances = new Distance[localTracks.Count, mbTracks.Count];
            var costs = new double[localTracks.Count, mbTracks.Count];

            for (int col = 0; col < mbTracks.Count; col++)
            {
                var totalTrackNumber = DistanceCalculator.GetTotalTrackNumber(mbTracks[col], mbTracks);
                for (int row = 0; row < localTracks.Count; row++)
                {
                    distances[row, col] = DistanceCalculator.TrackDistance(localTracks[row], mbTracks[col], totalTrackNumber, false);
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
    }
}
