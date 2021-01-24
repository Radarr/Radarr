using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata
        where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }

        public ValidationResult Test()
        {
            return new ValidationResult();
        }

        public virtual string GetFilenameAfterMove(Author author, BookFile bookFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(author.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(bookFile.Path, extension);

            return newFileName;
        }

        public virtual string GetFilenameAfterMove(Author author, string bookPath, MetadataFile metadataFile)
        {
            var existingFilename = Path.GetFileName(metadataFile.RelativePath);
            var newFileName = Path.Combine(author.Path, bookPath, existingFilename);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Author author, string path);

        public abstract MetadataFileResult AuthorMetadata(Author author);
        public abstract MetadataFileResult BookMetadata(Author author, BookFile bookFile);
        public abstract List<ImageFileResult> AuthorImages(Author author);
        public abstract List<ImageFileResult> BookImages(Author author, BookFile bookFile);

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
