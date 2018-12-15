using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
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
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly Logger _logger;

        public AddAlbumService(IAlbumService albumService,
                               IProvideAlbumInfo albumInfo,
                               IArtistMetadataRepository artistMetadataRepository,
                               IRefreshTrackService refreshTrackService,
                               Logger logger)
        {
            _albumService = albumService;
            _albumInfo = albumInfo;
            _artistMetadataRepository = artistMetadataRepository;
            _refreshTrackService = refreshTrackService;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            Ensure.That(newAlbum, () => newAlbum).IsNotNull();

            var tuple = AddSkyhookData(newAlbum);
            newAlbum = tuple.Item2;
            _logger.ProgressInfo("Adding Album {0}", newAlbum.Title);
            _artistMetadataRepository.UpsertMany(tuple.Item3);
            _albumService.AddAlbum(newAlbum, tuple.Item1);
            _refreshTrackService.RefreshTrackInfo(newAlbum);

            return newAlbum;
        }

        public List<Album> AddAlbums(List<Album> newAlbums)
        {
            var added = DateTime.UtcNow;
            var albumsToAdd = new List<Album>();

            foreach (var newAlbum in newAlbums)
            {
                var tuple = AddSkyhookData(newAlbum);
                var album = tuple.Item2;
                album.Added = added;
                album.LastInfoSync = added;
                _logger.ProgressInfo("Adding Album {0}", newAlbum.Title);
                _artistMetadataRepository.UpsertMany(tuple.Item3);
                album = _albumService.AddAlbum(album, tuple.Item1);
                _refreshTrackService.RefreshTrackInfo(album);
                albumsToAdd.Add(album);
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
