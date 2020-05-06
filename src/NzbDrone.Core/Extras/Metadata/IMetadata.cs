using System.Collections.Generic;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Author artist, BookFile trackFile, MetadataFile metadataFile);
        string GetFilenameAfterMove(Author artist, string albumPath, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Author artist, string path);
        MetadataFileResult ArtistMetadata(Author artist);
        MetadataFileResult AlbumMetadata(Author artist, Book album, string albumPath);
        MetadataFileResult TrackMetadata(Author artist, BookFile trackFile);
        List<ImageFileResult> ArtistImages(Author artist);
        List<ImageFileResult> AlbumImages(Author artist, Book album, string albumPath);
        List<ImageFileResult> TrackImages(Author artist, BookFile trackFile);
    }
}
