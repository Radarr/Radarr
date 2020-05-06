using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
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

        public virtual string GetFilenameAfterMove(Author artist, BookFile trackFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(artist.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(trackFile.Path, extension);

            return newFileName;
        }

        public virtual string GetFilenameAfterMove(Author artist, string albumPath, MetadataFile metadataFile)
        {
            var existingFilename = Path.GetFileName(metadataFile.RelativePath);
            var newFileName = Path.Combine(artist.Path, albumPath, existingFilename);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Author artist, string path);

        public abstract MetadataFileResult ArtistMetadata(Author artist);
        public abstract MetadataFileResult AlbumMetadata(Author artist, Book album, string albumPath);
        public abstract MetadataFileResult TrackMetadata(Author artist, BookFile trackFile);
        public abstract List<ImageFileResult> ArtistImages(Author artist);
        public abstract List<ImageFileResult> AlbumImages(Author artist, Book album, string albumPath);
        public abstract List<ImageFileResult> TrackImages(Author artist, BookFile trackFile);

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
