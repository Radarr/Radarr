using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistService
    {
        Author AddArtist(Author newArtist, bool doRefresh = true);
        List<Author> AddArtists(List<Author> newArtists, bool doRefresh = true);
    }

    public class AddArtistService : IAddArtistService
    {
        private readonly IArtistService _artistService;
        private readonly IArtistMetadataService _artistMetadataService;
        private readonly IProvideAuthorInfo _artistInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddArtistValidator _addArtistValidator;
        private readonly Logger _logger;

        public AddArtistService(IArtistService artistService,
                                IArtistMetadataService artistMetadataService,
                                IProvideAuthorInfo artistInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddArtistValidator addArtistValidator,
                                Logger logger)
        {
            _artistService = artistService;
            _artistMetadataService = artistMetadataService;
            _artistInfo = artistInfo;
            _fileNameBuilder = fileNameBuilder;
            _addArtistValidator = addArtistValidator;
            _logger = logger;
        }

        public Author AddArtist(Author newArtist, bool doRefresh = true)
        {
            Ensure.That(newArtist, () => newArtist).IsNotNull();

            newArtist = AddSkyhookData(newArtist);
            newArtist = SetPropertiesAndValidate(newArtist);

            _logger.Info("Adding Artist {0} Path: [{1}]", newArtist, newArtist.Path);

            // add metadata
            _artistMetadataService.Upsert(newArtist.Metadata.Value);
            newArtist.AuthorMetadataId = newArtist.Metadata.Value.Id;

            // add the artist itself
            _artistService.AddArtist(newArtist, doRefresh);

            return newArtist;
        }

        public List<Author> AddArtists(List<Author> newArtists, bool doRefresh = true)
        {
            var added = DateTime.UtcNow;
            var artistsToAdd = new List<Author>();

            foreach (var s in newArtists)
            {
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
                    _logger.Error(ex, "Failed to import id: {0} - {1}", s.Metadata.Value.ForeignAuthorId, s.Metadata.Value.Name);
                }
            }

            // add metadata
            _artistMetadataService.UpsertMany(artistsToAdd.Select(x => x.Metadata.Value).ToList());
            artistsToAdd.ForEach(x => x.AuthorMetadataId = x.Metadata.Value.Id);

            return _artistService.AddArtists(artistsToAdd, doRefresh);
        }

        private Author AddSkyhookData(Author newArtist)
        {
            Author artist;

            try
            {
                artist = _artistInfo.GetAuthorInfo(newArtist.Metadata.Value.ForeignAuthorId);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error("ReadarrId {0} was not found, it may have been removed from Musicbrainz.", newArtist.Metadata.Value.ForeignAuthorId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("MusicbrainzId", "An artist with this ID was not found", newArtist.Metadata.Value.ForeignAuthorId)
                                              });
            }

            artist.ApplyChanges(newArtist);

            return artist;
        }

        private Author SetPropertiesAndValidate(Author newArtist)
        {
            var path = newArtist.Path;
            if (string.IsNullOrWhiteSpace(path))
            {
                var folderName = _fileNameBuilder.GetArtistFolder(newArtist);
                path = Path.Combine(newArtist.RootFolderPath, folderName);
            }

            // Disambiguate artist path if it exists already
            if (_artistService.ArtistPathExists(path))
            {
                if (newArtist.Metadata.Value.Disambiguation.IsNotNullOrWhiteSpace())
                {
                    path += $" ({newArtist.Metadata.Value.Disambiguation})";
                }

                if (_artistService.ArtistPathExists(path))
                {
                    var basepath = path;
                    int i = 0;
                    do
                    {
                        i++;
                        path = basepath + $" ({i})";
                    }
                    while (_artistService.ArtistPathExists(path));
                }
            }

            newArtist.Path = path;
            newArtist.CleanName = newArtist.Metadata.Value.Name.CleanArtistName();
            newArtist.SortName = Parser.Parser.NormalizeTitle(newArtist.Metadata.Value.Name).ToLower();
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
