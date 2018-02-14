using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> GetDefaultDefinitions()
        {
            return new List<ProviderDefinition>();
        }

        public ProviderDefinition Definition { get; set; }

        public ValidationResult Test()
        {
            return new ValidationResult();
        }

        public virtual string GetFilenameAfterMove(Series series, EpisodeFile episodeFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(series.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(Path.Combine(series.Path, episodeFile.RelativePath), extension);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Series series, string path);

        public abstract MetadataFileResult SeriesMetadata(Series series);
        public abstract MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile);
        public abstract List<ImageFileResult> SeriesImages(Series series);
        public abstract List<ImageFileResult> SeasonImages(Series series, Season season);
        public abstract List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile);

        // For Movies
        public virtual string GetFilenameAfterMove(Movie movie, MovieFile movieFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(movie.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(Path.Combine(movie.Path, movieFile.RelativePath), extension);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Movie movie, string path);
        public abstract MetadataFileResult MovieMetadata(Movie movie);
        public abstract List<ImageFileResult> MovieImages(Movie movie);
        // End Movies

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
