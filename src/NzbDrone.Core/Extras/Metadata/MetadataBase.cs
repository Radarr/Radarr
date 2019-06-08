using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata where TSettings : IProviderConfig, new()
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

        public virtual string GetFilenameAfterMove(Artist artist, TrackFile trackFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(artist.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(trackFile.Path, extension);

            return newFileName;
        }

        public virtual string GetFilenameAfterMove(Artist artist, string albumPath, MetadataFile metadataFile)
        {
            var existingFilename = Path.GetFileName(metadataFile.RelativePath);
            var newFileName = Path.Combine(artist.Path, albumPath, existingFilename);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Artist artist, string path);

        public abstract MetadataFileResult ArtistMetadata(Artist artist);
        public abstract MetadataFileResult AlbumMetadata(Artist artist, Album album, string albumPath);
        public abstract MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile);
        public abstract List<ImageFileResult> ArtistImages(Artist artist);
        public abstract List<ImageFileResult> AlbumImages(Artist artist, Album album, string albumPath);
        public abstract List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile);

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
