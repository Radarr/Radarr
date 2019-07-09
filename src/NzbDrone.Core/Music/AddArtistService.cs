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
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IProvideArtistInfo _artistInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddArtistValidator _addArtistValidator;
        private readonly Logger _logger;

        public AddArtistService(IArtistService artistService,
                                IArtistMetadataRepository artistMetadataRepository,
                                IProvideArtistInfo artistInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddArtistValidator addArtistValidator,
                                Logger logger)
        {
            _artistService = artistService;
            _artistMetadataRepository = artistMetadataRepository;
            _artistInfo = artistInfo;
            _fileNameBuilder = fileNameBuilder;
            _addArtistValidator = addArtistValidator;
            _logger = logger;
        }

        public Artist AddArtist(Artist newArtist)
        {
            Ensure.That(newArtist, () => newArtist).IsNotNull();

            newArtist = AddSkyhookData(newArtist);
            newArtist = SetPropertiesAndValidate(newArtist);

            _logger.Info("Adding Artist {0} Path: [{1}]", newArtist, newArtist.Path);

            // add metadata
            _artistMetadataRepository.Upsert(newArtist.Metadata.Value);
            newArtist.ArtistMetadataId = newArtist.Metadata.Value.Id;

            // add the artist itself
            _artistService.AddArtist(newArtist);

            return newArtist;
        }

        public List<Artist> AddArtists(List<Artist> newArtists)
        {
            var added = DateTime.UtcNow;
            var artistsToAdd = new List<Artist>();

            foreach (var s in newArtists)
            {
                // TODO: Verify if adding skyhook data will be slow
                try
                {
                    var artist = AddSkyhookData(s);
                    artist = SetPropertiesAndValidate(artist);
                    artist.Added = added;
                    artistsToAdd.Add(artist);
                }
                catch (Exception ex)
                {
                    // Catch Import Errors for now until we get things fixed up
                    _logger.Error(ex, "Failed to import id: {0} - {1}", s.Metadata.Value.ForeignArtistId, s.Metadata.Value.Name);
                }
                
            }

            // add metadata
            _artistMetadataRepository.UpsertMany(artistsToAdd);

            _logger.Debug("metadata id 1 {0}", string.Join(", ", artistsToAdd.Select(x => x.Metadata.Value.Id)));
            _logger.Debug("metadata id 2 {0}", string.Join(", ", artistsToAdd.Select(x => x.ArtistMetadataId)));

            return _artistService.AddArtists(artistsToAdd);
        }

        private Artist AddSkyhookData(Artist newArtist)
        {
            Artist artist;

            try
            {
                artist = _artistInfo.GetArtistInfo(newArtist.Metadata.Value.ForeignArtistId, newArtist.MetadataProfileId);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error("LidarrId {0} was not found, it may have been removed from Lidarr.", newArtist.Metadata.Value.ForeignArtistId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("SpotifyId", "An artist with this ID was not found", newArtist.Metadata.Value.ForeignArtistId)
                                              });
            }

            // If albums were passed in on the new artist use them, otherwise use the albums from Skyhook
            if (newArtist.Albums == null || newArtist.Albums.Value == null || !newArtist.Albums.Value.Any())
            {
                newArtist.Albums = artist.Albums.Value;
            }

            artist.ApplyChanges(newArtist);

            return artist;
        }

        private Artist SetPropertiesAndValidate(Artist newArtist)
        {
            if (string.IsNullOrWhiteSpace(newArtist.Path))
            {
                var folderName = _fileNameBuilder.GetArtistFolder(newArtist);
                newArtist.Path = Path.Combine(newArtist.RootFolderPath, folderName);
            }

            newArtist.CleanName = newArtist.Metadata.Value.Name.CleanArtistName();
            newArtist.SortName = ArtistNameNormalizer.Normalize(newArtist.Metadata.Value.Name, newArtist.Metadata.Value.ForeignArtistId);
            newArtist.Added = DateTime.UtcNow;

            if (newArtist.AddOptions != null && newArtist.AddOptions.Monitor == MonitorTypes.None)
            {
                newArtist.Monitored = false;
            }

            var validationResult = _addArtistValidator.Validate(newArtist);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newArtist;
        }
    }
}
