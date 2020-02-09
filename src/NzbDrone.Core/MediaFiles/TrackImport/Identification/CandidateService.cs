using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public interface ICandidateService
    {
        List<CandidateAlbumRelease> GetDbCandidatesFromTags(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting);
        List<CandidateAlbumRelease> GetDbCandidatesFromFingerprint(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting);
        List<CandidateAlbumRelease> GetRemoteCandidates(LocalAlbumRelease localAlbumRelease);
    }

    public class CandidateService : ICandidateService
    {
        private readonly ISearchForNewAlbum _albumSearchService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public CandidateService(ISearchForNewAlbum albumSearchService,
                                IArtistService artistService,
                                IAlbumService albumService,
                                IReleaseService releaseService,
                                IMediaFileService mediaFileService,
                                Logger logger)
        {
            _albumSearchService = albumSearchService;
            _artistService = artistService;
            _albumService = albumService;
            _releaseService = releaseService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public List<CandidateAlbumRelease> GetDbCandidatesFromTags(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting)
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
                    tagCandidate = GetDbCandidatesByRelease(new List<AlbumRelease> { tagMbidRelease }, includeExisting);
                }
            }

            if (idOverrides?.AlbumRelease != null)
            {
                // this case overrides the release picked up from the file tags
                var release = idOverrides.AlbumRelease;
                _logger.Debug("Release {0} [{1} tracks] was forced", release, release.TrackCount);
                candidateReleases = GetDbCandidatesByRelease(new List<AlbumRelease> { release }, includeExisting);
            }
            else if (idOverrides?.Album != null)
            {
                // use the release from file tags if it exists and agrees with the specified album
                if (tagMbidRelease?.AlbumId == idOverrides.Album.Id)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetDbCandidatesByAlbum(localAlbumRelease, idOverrides.Album, includeExisting);
                }
            }
            else if (idOverrides?.Artist != null)
            {
                // use the release from file tags if it exists and agrees with the specified album
                if (tagMbidRelease?.Album.Value.ArtistMetadataId == idOverrides.Artist.ArtistMetadataId)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetDbCandidatesByArtist(localAlbumRelease, idOverrides.Artist, includeExisting);
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
                    candidateReleases = GetDbCandidates(localAlbumRelease, includeExisting);
                }
            }

            watch.Stop();
            _logger.Debug($"Getting candidates from tags for {localAlbumRelease.LocalTracks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            // if we haven't got any candidates then try fingerprinting
            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetDbCandidatesByRelease(List<AlbumRelease> releases, bool includeExisting)
        {
            // get the local tracks on disk for each album
            var albumTracks = releases.Select(x => x.AlbumId)
                .Distinct()
                .ToDictionary(id => id, id => includeExisting ? _mediaFileService.GetFilesByAlbum(id) : new List<TrackFile>());

            return releases.Select(x => new CandidateAlbumRelease
            {
                AlbumRelease = x,
                ExistingTracks = albumTracks[x.AlbumId]
            }).ToList();
        }

        private List<CandidateAlbumRelease> GetDbCandidatesByAlbum(LocalAlbumRelease localAlbumRelease, Album album, bool includeExisting)
        {
            // sort candidate releases by closest track count so that we stand a chance of
            // getting a perfect match early on
            return GetDbCandidatesByRelease(_releaseService.GetReleasesByAlbum(album.Id)
                                          .OrderBy(x => Math.Abs(localAlbumRelease.TrackCount - x.TrackCount))
                                          .ToList(), includeExisting);
        }

        private List<CandidateAlbumRelease> GetDbCandidatesByArtist(LocalAlbumRelease localAlbumRelease, Artist artist, bool includeExisting)
        {
            _logger.Trace("Getting candidates for {0}", artist);
            var candidateReleases = new List<CandidateAlbumRelease>();

            var albumTag = localAlbumRelease.LocalTracks.MostCommon(x => x.FileTrackInfo.AlbumTitle) ?? "";
            if (albumTag.IsNotNullOrWhiteSpace())
            {
                var possibleAlbums = _albumService.GetCandidates(artist.ArtistMetadataId, albumTag);
                foreach (var album in possibleAlbums)
                {
                    candidateReleases.AddRange(GetDbCandidatesByAlbum(localAlbumRelease, album, includeExisting));
                }
            }

            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetDbCandidates(LocalAlbumRelease localAlbumRelease, bool includeExisting)
        {
            // most general version, nothing has been specified.
            // get all plausible artists, then all plausible albums, then get releases for each of these.
            var candidateReleases = new List<CandidateAlbumRelease>();

            // check if it looks like VA.
            if (TrackGroupingService.IsVariousArtists(localAlbumRelease.LocalTracks))
            {
                var va = _artistService.FindById(DistanceCalculator.VariousArtistIds[0]);
                if (va != null)
                {
                    candidateReleases.AddRange(GetDbCandidatesByArtist(localAlbumRelease, va, includeExisting));
                }
            }

            var artistTag = localAlbumRelease.LocalTracks.MostCommon(x => x.FileTrackInfo.ArtistTitle) ?? "";
            if (artistTag.IsNotNullOrWhiteSpace())
            {
                var possibleArtists = _artistService.GetCandidates(artistTag);
                foreach (var artist in possibleArtists)
                {
                    candidateReleases.AddRange(GetDbCandidatesByArtist(localAlbumRelease, artist, includeExisting));
                }
            }

            return candidateReleases;
        }

        public List<CandidateAlbumRelease> GetDbCandidatesFromFingerprint(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting)
        {
            var recordingIds = localAlbumRelease.LocalTracks.Where(x => x.AcoustIdResults != null).SelectMany(x => x.AcoustIdResults).ToList();
            var allReleases = _releaseService.GetReleasesByRecordingIds(recordingIds);

            // make sure releases are consistent with those selected by the user
            if (idOverrides?.AlbumRelease != null)
            {
                allReleases = allReleases.Where(x => x.Id == idOverrides.AlbumRelease.Id).ToList();
            }
            else if (idOverrides?.Album != null)
            {
                allReleases = allReleases.Where(x => x.AlbumId == idOverrides.Album.Id).ToList();
            }
            else if (idOverrides?.Artist != null)
            {
                allReleases = allReleases.Where(x => x.Album.Value.ArtistMetadataId == idOverrides.Artist.ArtistMetadataId).ToList();
            }

            return GetDbCandidatesByRelease(allReleases.Select(x => new
            {
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

        public List<CandidateAlbumRelease> GetRemoteCandidates(LocalAlbumRelease localAlbumRelease)
        {
            // Gets candidate album releases from the metadata server.
            // Will eventually need adding locally if we find a match
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<Album> remoteAlbums;
            var candidates = new List<CandidateAlbumRelease>();

            var albumIds = localAlbumRelease.LocalTracks.Select(x => x.FileTrackInfo.AlbumMBId).Distinct().ToList();
            var recordingIds = localAlbumRelease.LocalTracks.Where(x => x.AcoustIdResults != null).SelectMany(x => x.AcoustIdResults).Distinct().ToList();

            try
            {
                if (albumIds.Count == 1 && albumIds[0].IsNotNullOrWhiteSpace())
                {
                    // Use mbids in tags if set
                    remoteAlbums = _albumSearchService.SearchForNewAlbum($"mbid:{albumIds[0]}", null);
                }
                else if (recordingIds.Any())
                {
                    // If fingerprints present use those
                    remoteAlbums = _albumSearchService.SearchForNewAlbumByRecordingIds(recordingIds);
                }
                else
                {
                    // fall back to artist / album name search
                    string artistTag;

                    if (TrackGroupingService.IsVariousArtists(localAlbumRelease.LocalTracks))
                    {
                        artistTag = "Various Artists";
                    }
                    else
                    {
                        artistTag = localAlbumRelease.LocalTracks.MostCommon(x => x.FileTrackInfo.ArtistTitle) ?? "";
                    }

                    var albumTag = localAlbumRelease.LocalTracks.MostCommon(x => x.FileTrackInfo.AlbumTitle) ?? "";

                    if (artistTag.IsNullOrWhiteSpace() || albumTag.IsNullOrWhiteSpace())
                    {
                        return candidates;
                    }

                    remoteAlbums = _albumSearchService.SearchForNewAlbum(albumTag, artistTag);
                }
            }
            catch (SkyHookException e)
            {
                _logger.Info(e, "Skipping album due to SkyHook error");
                remoteAlbums = new List<Album>();
            }

            foreach (var album in remoteAlbums)
            {
                // We have to make sure various bits and pieces are populated that are normally handled
                // by a database lazy load
                foreach (var release in album.AlbumReleases.Value)
                {
                    release.Album = album;
                    candidates.Add(new CandidateAlbumRelease
                    {
                        AlbumRelease = release,
                        ExistingTracks = new List<TrackFile>()
                    });
                }
            }

            watch.Stop();
            _logger.Debug($"Getting {candidates.Count} remote candidates from tags for {localAlbumRelease.LocalTracks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            return candidates;
        }
    }
}
