using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public interface IIdentificationService
    {
        List<LocalAlbumRelease> Identify(List<LocalTrack> localTracks, IdentificationOverrides idOverrides, ImportDecisionMakerConfig config);
    }

    public class IdentificationService : IIdentificationService
    {
        private readonly ITrackGroupingService _trackGroupingService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAugmentingService _augmentingService;
        private readonly ICandidateService _candidateService;
        private readonly Logger _logger;

        public IdentificationService(ITrackGroupingService trackGroupingService,
                                     IAudioTagService audioTagService,
                                     IAugmentingService augmentingService,
                                     ICandidateService candidateService,
                                     Logger logger)
        {
            _trackGroupingService = trackGroupingService;
            _audioTagService = audioTagService;
            _augmentingService = augmentingService;
            _candidateService = candidateService;
            _logger = logger;
        }

        public List<LocalAlbumRelease> GetLocalAlbumReleases(List<LocalTrack> localTracks, bool singleRelease)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<LocalAlbumRelease> releases;
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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _logger.Debug("Starting track identification");

            var releases = GetLocalAlbumReleases(localTracks, config.SingleRelease);

            int i = 0;
            foreach (var localRelease in releases)
            {
                i++;
                _logger.ProgressInfo($"Identifying book {i}/{releases.Count}");
                IdentifyRelease(localRelease, idOverrides, config);
            }

            watch.Stop();

            _logger.Debug($"Track identification for {localTracks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            return releases;
        }

        private List<LocalTrack> ToLocalTrack(IEnumerable<BookFile> trackfiles, LocalAlbumRelease localRelease)
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

            var candidateReleases = _candidateService.GetDbCandidatesFromTags(localAlbumRelease, idOverrides, config.IncludeExisting);

            if (candidateReleases.Count == 0 && config.AddNewArtists)
            {
                candidateReleases = _candidateService.GetRemoteCandidates(localAlbumRelease);
            }

            if (candidateReleases.Count == 0)
            {
                return;
            }

            _logger.Debug($"Got {candidateReleases.Count} candidates for {localAlbumRelease.LocalTracks.Count} tracks in {watch.ElapsedMilliseconds}ms");

            // convert all the TrackFiles that represent extra files to List<LocalTrack>
            var allLocalTracks = ToLocalTrack(candidateReleases
                                              .SelectMany(x => x.ExistingTracks)
                                              .DistinctBy(x => x.Path), localAlbumRelease);

            _logger.Debug($"Retrieved {allLocalTracks.Count} possible tracks in {watch.ElapsedMilliseconds}ms");

            GetBestRelease(localAlbumRelease, candidateReleases, allLocalTracks);

            _logger.Debug($"Best release found in {watch.ElapsedMilliseconds}ms");

            localAlbumRelease.PopulateMatch();

            _logger.Debug($"IdentifyRelease done in {watch.ElapsedMilliseconds}ms");
        }

        private void GetBestRelease(LocalAlbumRelease localAlbumRelease, List<CandidateAlbumRelease> candidateReleases, List<LocalTrack> extraTracksOnDisk)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _logger.Debug("Matching {0} track files against {1} candidates", localAlbumRelease.TrackCount, candidateReleases.Count);
            _logger.Trace("Processing files:\n{0}", string.Join("\n", localAlbumRelease.LocalTracks.Select(x => x.Path)));

            double bestDistance = 1.0;

            foreach (var candidateRelease in candidateReleases)
            {
                var release = candidateRelease.Book;
                _logger.Debug($"Trying Release {release}");
                var rwatch = System.Diagnostics.Stopwatch.StartNew();

                var extraTrackPaths = candidateRelease.ExistingTracks.Select(x => x.Path).ToList();
                var extraTracks = extraTracksOnDisk.Where(x => extraTrackPaths.Contains(x.Path)).ToList();
                var allLocalTracks = localAlbumRelease.LocalTracks.Concat(extraTracks).DistinctBy(x => x.Path).ToList();

                var distance = DistanceCalculator.BookDistance(allLocalTracks, release);
                var currDistance = distance.NormalizedDistance();

                rwatch.Stop();
                _logger.Debug("Release {0} has distance {1} vs best distance {2} [{3}ms]",
                              release,
                              currDistance,
                              bestDistance,
                              rwatch.ElapsedMilliseconds);
                if (currDistance < bestDistance)
                {
                    bestDistance = currDistance;
                    localAlbumRelease.Distance = distance;
                    localAlbumRelease.Book = release;
                    localAlbumRelease.ExistingTracks = extraTracks;
                    if (currDistance == 0.0)
                    {
                        break;
                    }
                }
            }

            watch.Stop();
            _logger.Debug($"Best release: {localAlbumRelease.Book} Distance {localAlbumRelease.Distance.NormalizedDistance()} found in {watch.ElapsedMilliseconds}ms");
        }
    }
}
