using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Music
{
    public interface IAddAlbumService
    {
        Album AddAlbum(Album newAlbum);
        List<Album> AddAlbums(List<Album> newAlbums, bool ignoreErrors = false);
    }

    public class AddAlbumService : IAddAlbumService
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IAddAlbumValidator _addAlbumValidator;
        private readonly Logger _logger;

        public AddAlbumService(IAlbumService albumService,
                               IArtistService artistService,
                               IAddAlbumValidator addAlbumValidator,
                               Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _addAlbumValidator = addAlbumValidator;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            Ensure.That(newAlbum, () => newAlbum).IsNotNull();

            newAlbum = SetPropertiesAndValidate(newAlbum);

            _logger.Info("Adding Album {0} Path: [{1}]", newAlbum, newAlbum.Path.SanitizeForLog());

            _albumService.AddAlbum(newAlbum);

            return newAlbum;
        }

        public List<Album> AddAlbums(List<Album> newAlbums, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var albumsToAdd = new List<Album>();

            foreach (var a in newAlbums)
            {
                _logger.Info("Adding Album {0}", a);

                try
                {
                    var album = SetPropertiesAndValidate(a);
                    album.Added = added;
                    albumsToAdd.Add(album);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug(ex, "Album {0} was not added due to validation failures.", a.Title);
                }
            }

            return _albumService.AddAlbums(albumsToAdd);
        }

        private Album SetPropertiesAndValidate(Album newAlbum)
        {
            if (string.IsNullOrWhiteSpace(newAlbum.Path) && newAlbum.ArtistId.HasValue)
            {
                var artist = _artistService.GetArtist(newAlbum.ArtistId.Value);
                var folderName = GetAlbumFolder(newAlbum);
                newAlbum.Path = Path.Combine(artist.Path, folderName);
            }

            if (string.IsNullOrWhiteSpace(newAlbum.SortTitle))
            {
                newAlbum.SortTitle = newAlbum.Title?.ToLowerInvariant();
            }

            newAlbum.Added = DateTime.UtcNow;

            var validationResult = _addAlbumValidator.Validate(newAlbum);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newAlbum;
        }

        private static string GetAlbumFolder(Album album)
        {
            var title = album.Title ?? "Unknown Album";
            var year = album.ReleaseDate?.Year;

            if (year.HasValue)
            {
                return FileNameBuilder.CleanFileName($"{title} ({year})");
            }

            return FileNameBuilder.CleanFileName(title);
        }
    }
}
