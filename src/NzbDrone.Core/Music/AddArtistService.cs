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
    }

    public class AddSeriesService : IAddArtistService
    {
        private readonly IArtistService _artistService;
        private readonly IProvideArtistInfo _artistInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddArtistValidator _addArtistValidator;
        private readonly Logger _logger;

        public AddSeriesService(IArtistService artistService,
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

            newArtist = AddSkyhookData(newArtist);

            if (string.IsNullOrWhiteSpace(newArtist.Path))
            {
                var folderName = newArtist.ArtistName;// _fileNameBuilder.GetArtistFolder(newArtist);
                newArtist.Path = Path.Combine(newArtist.RootFolderPath, folderName);
            }

            newArtist.CleanTitle = newArtist.ArtistName.CleanSeriesTitle();
            newArtist.SortTitle = ArtistNameNormalizer.Normalize(newArtist.ArtistName, newArtist.ItunesId);
            newArtist.Added = DateTime.UtcNow;

            var validationResult = _addArtistValidator.Validate(newArtist);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            _logger.Info("Adding Series {0} Path: [{1}]", newArtist, newArtist.Path);
            _artistService.AddArtist(newArtist);

            return newArtist;
        }

        private Artist AddSkyhookData(Artist newArtist)
        {
            Tuple<Artist, List<Track>> tuple;

            try
            {
                tuple = _artistInfo.GetArtistInfo(newArtist.ItunesId);
            }
            catch (SeriesNotFoundException)
            {
                _logger.Error("tvdbid {1} was not found, it may have been removed from TheTVDB.", newArtist.ItunesId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TvdbId", "A series with this ID was not found", newArtist.ItunesId)
                                              });
            }

            var artist = tuple.Item1;

            // If seasons were passed in on the new series use them, otherwise use the seasons from Skyhook
            // TODO: Refactor for albums
            newArtist.Albums = newArtist.Albums != null && newArtist.Albums.Any() ? newArtist.Albums : artist.Albums;

            artist.ApplyChanges(newArtist);

            return artist;
        }
    }
}
