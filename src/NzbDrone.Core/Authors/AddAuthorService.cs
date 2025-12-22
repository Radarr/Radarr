using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Authors
{
    public interface IAddAuthorService
    {
        Author AddAuthor(Author newAuthor);
        List<Author> AddAuthors(List<Author> newAuthors, bool ignoreErrors = false);
    }

    public class AddAuthorService : IAddAuthorService
    {
        private readonly IAuthorService _authorService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddAuthorValidator _addAuthorValidator;
        private readonly Logger _logger;

        public AddAuthorService(IAuthorService authorService,
                                IBuildFileNames fileNameBuilder,
                                IAddAuthorValidator addAuthorValidator,
                                Logger logger)
        {
            _authorService = authorService;
            _fileNameBuilder = fileNameBuilder;
            _addAuthorValidator = addAuthorValidator;
            _logger = logger;
        }

        public Author AddAuthor(Author newAuthor)
        {
            Ensure.That(newAuthor, () => newAuthor).IsNotNull();

            newAuthor = SetPropertiesAndValidate(newAuthor);

            _logger.Info("Adding Author {0} Path: [{1}]", newAuthor, newAuthor.Path.SanitizeForLog());

            _authorService.AddAuthor(newAuthor);

            return newAuthor;
        }

        public List<Author> AddAuthors(List<Author> newAuthors, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var authorsToAdd = new List<Author>();

            foreach (var a in newAuthors)
            {
                if (a.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Author {0} Root Folder Path: [{1}]", a, a.RootFolderPath.SanitizeForLog());
                }
                else
                {
                    _logger.Info("Adding Author {0} Path: [{1}]", a, a.Path.SanitizeForLog());
                }

                try
                {
                    var author = SetPropertiesAndValidate(a);
                    author.Added = added;
                    authorsToAdd.Add(author);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Author {0} was not added due to validation failures. {1}", a.Name, ex.Message);
                }
            }

            return _authorService.AddAuthors(authorsToAdd);
        }

        private Author SetPropertiesAndValidate(Author newAuthor)
        {
            if (string.IsNullOrWhiteSpace(newAuthor.Path))
            {
                var folderName = GetAuthorFolder(newAuthor);
                newAuthor.Path = Path.Combine(newAuthor.RootFolderPath, folderName);
            }

            if (string.IsNullOrWhiteSpace(newAuthor.SortName))
            {
                newAuthor.SortName = newAuthor.Name?.ToLowerInvariant();
            }

            newAuthor.Added = DateTime.UtcNow;

            var validationResult = _addAuthorValidator.Validate(newAuthor);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newAuthor;
        }

        private string GetAuthorFolder(Author author)
        {
            var name = author.Name ?? "Unknown Author";
            return FileNameBuilder.CleanFileName(name);
        }
    }
}
