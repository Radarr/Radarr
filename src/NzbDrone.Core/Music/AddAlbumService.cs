using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
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
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly Logger _logger;

        public AddAlbumService(IAlbumService albumService,
                                IProvideAlbumInfo albumInfo,
                                IRefreshTrackService refreshTrackService,
                                Logger logger)
        {
            _albumService = albumService;
            _albumInfo = albumInfo;
            _refreshTrackService = refreshTrackService;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            Ensure.That(newAlbum, () => newAlbum).IsNotNull();

            var tuple = AddSkyhookData(newAlbum);
            newAlbum = tuple.Item1;
            _refreshTrackService.RefreshTrackInfo(newAlbum, tuple.Item2);
            _logger.Info("Adding Album {0}", newAlbum);
            _albumService.AddAlbum(newAlbum);

            return newAlbum;
        }

        public List<Album> AddAlbums(List<Album> newAlbums)
        {
            var added = DateTime.UtcNow;
            var albumsToAdd = new List<Album>();

            foreach (var newAlbum in newAlbums)
            {
                var tuple = AddSkyhookData(newAlbum);
                var album = tuple.Item1;
                album.Added = added;
                album.LastInfoSync = added;
                album = _albumService.AddAlbum(album);
                _refreshTrackService.RefreshTrackInfo(album,tuple.Item2);
                albumsToAdd.Add(album);
            }

            return albumsToAdd;
        }

        private Tuple<Album, List<Track>> AddSkyhookData(Album newAlbum)
        {
            Tuple<Album, List<Track>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(newAlbum.ForeignAlbumId, null);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error("LidarrId {1} was not found, it may have been removed from Lidarr.", newAlbum.ForeignAlbumId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("MusicBrainzId", "An album with this ID was not found", newAlbum.ForeignAlbumId)
                                              });
            }

            tuple.Item1.ArtistId = newAlbum.ArtistId;
            tuple.Item1.Monitored = newAlbum.Monitored;
            tuple.Item1.ProfileId = newAlbum.ProfileId;
            tuple.Item1.Duration = tuple.Item2.Sum(track => track.Duration);

            return tuple;
        }
    }
}
