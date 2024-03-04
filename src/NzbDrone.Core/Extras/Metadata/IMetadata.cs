using System.Collections.Generic;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Movie movie, MovieFile movieFile, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Movie movie, string path);
        MetadataFileResult MovieMetadata(Movie movie, MovieFile movieFile);
        List<ImageFileResult> MovieImages(Movie movie, MovieFile movieFile);
    }
}
