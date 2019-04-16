using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.Music
{
    public interface IAddAlbumService
    {
        Album AddAlbum(Album newAlbum);
        List<Album> AddAlbums(List<Album> newAlbums);
    }

    public class AddAlbumService : IAddAlbumService
    {
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly Logger _logger;

        public AddAlbumService(IAlbumService albumService,
                               IReleaseService releaseService,
                               IProvideAlbumInfo albumInfo,
                               IArtistMetadataRepository artistMetadataRepository,
                               IRefreshTrackService refreshTrackService,
                               Logger logger)
        {
            _albumService = albumService;
            _releaseService = releaseService;
            _albumInfo = albumInfo;
            _artistMetadataRepository = artistMetadataRepository;
            _refreshTrackService = refreshTrackService;
            _logger = logger;
        }

        public List<AlbumRelease> AddAlbumReleases(Album album)
        {
            var remoteReleases = album.AlbumReleases.Value.DistinctBy(m => m.ForeignReleaseId).ToList();
            var existingReleases = _releaseService.GetReleasesForRefresh(album.Id, remoteReleases.Select(x => x.ForeignReleaseId));
            var newReleaseList = new List<AlbumRelease>();
            var updateReleaseList = new List<AlbumRelease>();
            
            foreach (var release in remoteReleases)
            {
                release.AlbumId = album.Id;
                release.Album = album;
                var releaseToRefresh = existingReleases.SingleOrDefault(r => r.ForeignReleaseId == release.ForeignReleaseId);

                if (releaseToRefresh != null)
                {
                    existingReleases.Remove(releaseToRefresh);

                    // copy across the db keys and check for equality
                    release.Id = releaseToRefresh.Id;
                    release.AlbumId = releaseToRefresh.AlbumId;

                    updateReleaseList.Add(release);
                }
                else
                {
                    newReleaseList.Add(release);
                }
            }
            
            // Ensure only one release is monitored
            remoteReleases.ForEach(x => x.Monitored = false);
            remoteReleases.OrderByDescending(x => x.TrackCount).First().Monitored = true;
            Ensure.That(remoteReleases.Count(x => x.Monitored) == 1).IsTrue();
            
            // Since this is a new album, we can't be deleting any existing releases
            _releaseService.UpdateMany(updateReleaseList);
            _releaseService.InsertMany(newReleaseList);
            
            return remoteReleases;
        }
        
        private Album AddAlbum(Tuple<string, Album, List<ArtistMetadata>> skyHookData)
        {
            var newAlbum = skyHookData.Item2;

            if (newAlbum.AlbumReleases.Value.Count == 0)
            {
                _logger.Debug($"Skipping album with no valid releases {newAlbum}");
                return null;
            }

            _logger.ProgressInfo("Adding Album {0}", newAlbum.Title);
            
            _artistMetadataRepository.UpsertMany(skyHookData.Item3);
            newAlbum.ArtistMetadata = _artistMetadataRepository.FindById(skyHookData.Item1);
            newAlbum.ArtistMetadataId = newAlbum.ArtistMetadata.Value.Id;
            
            _albumService.AddAlbum(newAlbum);
            AddAlbumReleases(newAlbum);

            _refreshTrackService.RefreshTrackInfo(newAlbum, false);

            return newAlbum;
        }
            
        public Album AddAlbum(Album newAlbum)
        {
            Ensure.That(newAlbum, () => newAlbum).IsNotNull();

            var tuple = AddSkyhookData(newAlbum);
            
            return AddAlbum(tuple);
        }

        public List<Album> AddAlbums(List<Album> newAlbums)
        {
            var added = DateTime.UtcNow;
            var albumsToAdd = new List<Album>();

            foreach (var newAlbum in newAlbums)
            {
                var tuple = AddSkyhookData(newAlbum);
                tuple.Item2.Added = added;
                tuple.Item2.LastInfoSync = added;
                
                albumsToAdd.Add(AddAlbum(tuple));
            }

            return albumsToAdd;
        }

        private Tuple<string, Album, List<ArtistMetadata>> AddSkyhookData(Album newAlbum)
        {
            Tuple<string, Album, List<ArtistMetadata>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(newAlbum.ForeignAlbumId);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error("LidarrId {1} was not found, it may have been removed from Lidarr.", newAlbum.ForeignAlbumId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("MusicBrainzId", "An album with this ID was not found", newAlbum.ForeignAlbumId)
                                              });
            }

            tuple.Item2.Monitored = newAlbum.Monitored;
            tuple.Item2.ProfileId = newAlbum.ProfileId;

            return tuple;
        }
    }
}
