using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Audiobooks
{
    public interface IAddAudiobookService
    {
        Audiobook AddAudiobook(Audiobook newAudiobook);
        List<Audiobook> AddAudiobooks(List<Audiobook> newAudiobooks, bool ignoreErrors = false);
    }

    public class AddAudiobookService : IAddAudiobookService
    {
        private readonly IAudiobookService _audiobookService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddAudiobookValidator _addAudiobookValidator;
        private readonly Logger _logger;

        public AddAudiobookService(IAudiobookService audiobookService,
                                   IBuildFileNames fileNameBuilder,
                                   IAddAudiobookValidator addAudiobookValidator,
                                   Logger logger)
        {
            _audiobookService = audiobookService;
            _fileNameBuilder = fileNameBuilder;
            _addAudiobookValidator = addAudiobookValidator;
            _logger = logger;
        }

        public Audiobook AddAudiobook(Audiobook newAudiobook)
        {
            Ensure.That(newAudiobook, () => newAudiobook).IsNotNull();

            newAudiobook = SetPropertiesAndValidate(newAudiobook);

            _logger.Info("Adding Audiobook {0} Path: [{1}]", newAudiobook, newAudiobook.Path.SanitizeForLog());

            _audiobookService.AddAudiobook(newAudiobook);

            return newAudiobook;
        }

        public List<Audiobook> AddAudiobooks(List<Audiobook> newAudiobooks, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var audiobooksToAdd = new List<Audiobook>();

            foreach (var a in newAudiobooks)
            {
                if (a.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Audiobook {0} Root Folder Path: [{1}]", a, a.RootFolderPath.SanitizeForLog());
                }
                else
                {
                    _logger.Info("Adding Audiobook {0} Path: [{1}]", a, a.Path.SanitizeForLog());
                }

                try
                {
                    var audiobook = SetPropertiesAndValidate(a);
                    audiobook.Added = added;
                    audiobooksToAdd.Add(audiobook);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Audiobook {0} was not added due to validation failures. {1}", a.Title, ex.Message);
                }
            }

            return _audiobookService.AddAudiobooks(audiobooksToAdd);
        }

        private Audiobook SetPropertiesAndValidate(Audiobook newAudiobook)
        {
            if (string.IsNullOrWhiteSpace(newAudiobook.Path))
            {
                var folderName = GetAudiobookFolder(newAudiobook);
                newAudiobook.Path = Path.Combine(newAudiobook.RootFolderPath, folderName);
            }

            if (string.IsNullOrWhiteSpace(newAudiobook.SortTitle))
            {
                newAudiobook.SortTitle = newAudiobook.Title?.ToLowerInvariant();
            }

            newAudiobook.Added = DateTime.UtcNow;

            var validationResult = _addAudiobookValidator.Validate(newAudiobook);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newAudiobook;
        }

        private string GetAudiobookFolder(Audiobook audiobook)
        {
            var title = audiobook.Title ?? "Unknown";
            var year = audiobook.ReleaseDate?.Year.ToString() ?? "Unknown";
            var narrator = audiobook.Narrator;

            if (!string.IsNullOrWhiteSpace(narrator))
            {
                return FileNameBuilder.CleanFileName($"{title} ({year}) [{narrator}]");
            }

            return FileNameBuilder.CleanFileName($"{title} ({year})");
        }
    }
}
