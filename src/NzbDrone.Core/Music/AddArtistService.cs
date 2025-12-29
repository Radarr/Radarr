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
    public interface IAddArtistService
    {
        Artist AddArtist(Artist newArtist);
        List<Artist> AddArtists(List<Artist> newArtists, bool ignoreErrors = false);
    }

    public class AddArtistService : IAddArtistService
    {
        private readonly IArtistService _artistService;
        private readonly IAddArtistValidator _addArtistValidator;
        private readonly Logger _logger;

        public AddArtistService(IArtistService artistService,
                                IAddArtistValidator addArtistValidator,
                                Logger logger)
        {
            _artistService = artistService;
            _addArtistValidator = addArtistValidator;
            _logger = logger;
        }

        public Artist AddArtist(Artist newArtist)
        {
            Ensure.That(newArtist, () => newArtist).IsNotNull();

            newArtist = SetPropertiesAndValidate(newArtist);

            _logger.Info("Adding Artist {0} Path: [{1}]", newArtist, newArtist.Path.SanitizeForLog());

            _artistService.AddArtist(newArtist);

            return newArtist;
        }

        public List<Artist> AddArtists(List<Artist> newArtists, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var artistsToAdd = new List<Artist>();

            foreach (var a in newArtists)
            {
                if (a.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Artist {0} Root Folder Path: [{1}]", a, a.RootFolderPath.SanitizeForLog());
                }
                else
                {
                    _logger.Info("Adding Artist {0} Path: [{1}]", a, a.Path.SanitizeForLog());
                }

                try
                {
                    var artist = SetPropertiesAndValidate(a);
                    artist.Added = added;
                    artistsToAdd.Add(artist);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug(ex, "Artist {0} was not added due to validation failures.", a.Name);
                }
            }

            return _artistService.AddArtists(artistsToAdd);
        }

        private Artist SetPropertiesAndValidate(Artist newArtist)
        {
            if (string.IsNullOrWhiteSpace(newArtist.Path))
            {
                var folderName = GetArtistFolder(newArtist);
                newArtist.Path = Path.Combine(newArtist.RootFolderPath, folderName);
            }

            if (string.IsNullOrWhiteSpace(newArtist.SortName))
            {
                newArtist.SortName = newArtist.Name?.ToLowerInvariant();
            }

            newArtist.Added = DateTime.UtcNow;

            var validationResult = _addArtistValidator.Validate(newArtist);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newArtist;
        }

        private static string GetArtistFolder(Artist artist)
        {
            var name = artist.Name ?? "Unknown Artist";
            return FileNameBuilder.CleanFileName(name);
        }
    }
}
