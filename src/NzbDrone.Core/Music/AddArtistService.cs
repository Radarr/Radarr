using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.MetadataSource.SkyHook;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistService
    {
        Artist AddArtist(Artist newArtist);
        List<Artist> AddArtists(List<Artist> newArtists);
    }

    public class AddArtistService : IAddArtistService
    {
        private readonly IArtistService _artistService;
        private readonly IProvideArtistInfo _artistInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddArtistValidator _addArtistValidator;
        private readonly Logger _logger;

        public AddArtistService(IArtistService artistService,
                                IProvideArtistInfo artistInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddArtistValidator addArtistValidator,
                                Logger logger)
        {
            _artistService = artistService;
            _artistInfo = artistInfo;
            _fileNameBuilder = fileNameBuilder;
            _addArtistValidator = addArtistValidator;
            _logger = logger;
        }

        public Artist AddArtist(Artist newArtist)
        {
            Ensure.That(newArtist, () => newArtist).IsNotNull();

            // Called for artist creation from API, duplicated call when called from Lidarr. Fix this.
            newArtist = AddSkyhookData(newArtist);

            if (string.IsNullOrWhiteSpace(newArtist.Path))
            {
                var folderName = _fileNameBuilder.GetArtistFolder(newArtist);
                newArtist.Path = Path.Combine(newArtist.RootFolderPath, folderName);
            }

            newArtist.CleanName = newArtist.Name.CleanArtistName();
            newArtist.SortName = ArtistNameNormalizer.Normalize(newArtist.Name, newArtist.ForeignArtistId); // There is no Sort Title
            newArtist.Added = DateTime.UtcNow;

            var validationResult = _addArtistValidator.Validate(newArtist);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            _logger.Info("Adding Artist {0} Path: [{1}]", newArtist, newArtist.Path);
            newArtist = _artistService.AddArtist(newArtist);

            return newArtist;
        }

        public List<Artist> AddArtists(List<Artist> newArtists)
        {
            newArtists.ForEach(artist =>
            {
                AddArtist(artist);
            });

            return newArtists;
        }

        private Artist AddSkyhookData(Artist newArtist)
        {
            Tuple<Artist, List<Album>> tuple;

            try
            {
                tuple = _artistInfo.GetArtistInfo(newArtist.ForeignArtistId, newArtist.PrimaryAlbumTypes, newArtist.SecondaryAlbumTypes);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error("LidarrId {1} was not found, it may have been removed from Lidarr.", newArtist.ForeignArtistId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("SpotifyId", "An artist with this ID was not found", newArtist.ForeignArtistId)
                                              });
            }

            var artist = tuple.Item1;

            // If albums were passed in on the new artist use them, otherwise use the albums from Skyhook
            newArtist.Albums = newArtist.Albums != null && newArtist.Albums.Any() ? newArtist.Albums : artist.Albums;

            artist.ApplyChanges(newArtist);

            return artist;
        }
    }
}
