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

namespace NzbDrone.Core.Books
{
    public interface IAddAuthorService
    {
        Author AddAuthor(Author newAuthor, bool doRefresh = true);
        List<Author> AddAuthors(List<Author> newAuthors, bool doRefresh = true);
    }

    public class AddArtistService : IAddAuthorService
    {
        private readonly IAuthorService _authorService;
        private readonly IAuthorMetadataService _authorMetadataService;
        private readonly IProvideAuthorInfo _authorInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddAuthorValidator _addAuthorValidator;
        private readonly Logger _logger;

        public AddArtistService(IAuthorService authorService,
                                IAuthorMetadataService authorMetadataService,
                                IProvideAuthorInfo authorInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddAuthorValidator addAuthorValidator,
                                Logger logger)
        {
            _authorService = authorService;
            _authorMetadataService = authorMetadataService;
            _authorInfo = authorInfo;
            _fileNameBuilder = fileNameBuilder;
            _addAuthorValidator = addAuthorValidator;
            _logger = logger;
        }

        public Author AddAuthor(Author newAuthor, bool doRefresh = true)
        {
            Ensure.That(newAuthor, () => newAuthor).IsNotNull();

            newAuthor = AddSkyhookData(newAuthor);
            newAuthor = SetPropertiesAndValidate(newAuthor);

            _logger.Info("Adding Author {0} Path: [{1}]", newAuthor, newAuthor.Path);

            // add metadata
            _authorMetadataService.Upsert(newAuthor.Metadata.Value);
            newAuthor.AuthorMetadataId = newAuthor.Metadata.Value.Id;

            // add the author itself
            _authorService.AddAuthor(newAuthor, doRefresh);

            return newAuthor;
        }

        public List<Author> AddAuthors(List<Author> newAuthors, bool doRefresh = true)
        {
            var added = DateTime.UtcNow;
            var authorsToAdd = new List<Author>();

            foreach (var s in newAuthors)
            {
                try
                {
                    var author = AddSkyhookData(s);
                    author = SetPropertiesAndValidate(author);
                    author.Added = added;
                    authorsToAdd.Add(author);
                }
                catch (Exception ex)
                {
                    // Catch Import Errors for now until we get things fixed up
                    _logger.Error(ex, "Failed to import id: {0} - {1}", s.Metadata.Value.ForeignAuthorId, s.Metadata.Value.Name);
                }
            }

            // add metadata
            _authorMetadataService.UpsertMany(authorsToAdd.Select(x => x.Metadata.Value).ToList());
            authorsToAdd.ForEach(x => x.AuthorMetadataId = x.Metadata.Value.Id);

            return _authorService.AddAuthors(authorsToAdd, doRefresh);
        }

        private Author AddSkyhookData(Author newAuthor)
        {
            Author author;

            try
            {
                author = _authorInfo.GetAuthorInfo(newAuthor.Metadata.Value.ForeignAuthorId);
            }
            catch (AuthorNotFoundException)
            {
                _logger.Error("ReadarrId {0} was not found, it may have been removed from Goodreads.", newAuthor.Metadata.Value.ForeignAuthorId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("MusicbrainzId", "An author with this ID was not found", newAuthor.Metadata.Value.ForeignAuthorId)
                                              });
            }

            author.ApplyChanges(newAuthor);

            return author;
        }

        private Author SetPropertiesAndValidate(Author newAuthor)
        {
            var path = newAuthor.Path;
            if (string.IsNullOrWhiteSpace(path))
            {
                var folderName = _fileNameBuilder.GetAuthorFolder(newAuthor);
                path = Path.Combine(newAuthor.RootFolderPath, folderName);
            }

            // Disambiguate author path if it exists already
            if (_authorService.AuthorPathExists(path))
            {
                if (newAuthor.Metadata.Value.Disambiguation.IsNotNullOrWhiteSpace())
                {
                    path += $" ({newAuthor.Metadata.Value.Disambiguation})";
                }

                if (_authorService.AuthorPathExists(path))
                {
                    var basepath = path;
                    int i = 0;
                    do
                    {
                        i++;
                        path = basepath + $" ({i})";
                    }
                    while (_authorService.AuthorPathExists(path));
                }
            }

            newAuthor.Path = path;
            newAuthor.CleanName = newAuthor.Metadata.Value.Name.CleanAuthorName();
            newAuthor.SortName = Parser.Parser.NormalizeTitle(newAuthor.Metadata.Value.Name).ToLower();
            newAuthor.Added = DateTime.UtcNow;

            if (newAuthor.AddOptions != null && newAuthor.AddOptions.Monitor == MonitorTypes.None)
            {
                newAuthor.Monitored = false;
            }

            var validationResult = _addAuthorValidator.Validate(newAuthor);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newAuthor;
        }
    }
}
