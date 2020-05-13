using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Author author, BookFile bookFile, MetadataFile metadataFile);
        string GetFilenameAfterMove(Author author, string albumPath, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Author author, string path);
        MetadataFileResult ArtistMetadata(Author author);
        MetadataFileResult AlbumMetadata(Author author, Book book, string albumPath);
        MetadataFileResult TrackMetadata(Author author, BookFile bookFile);
        List<ImageFileResult> ArtistImages(Author author);
        List<ImageFileResult> AlbumImages(Author author, Book book, string albumPath);
        List<ImageFileResult> TrackImages(Author author, BookFile bookFile);
    }
}
