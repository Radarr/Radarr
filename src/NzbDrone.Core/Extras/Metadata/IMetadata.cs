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
        string GetFilenameAfterMove(Author author, string bookPath, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Author author, string path);
        MetadataFileResult AuthorMetadata(Author author);
        MetadataFileResult BookMetadata(Author author, BookFile bookFile);
        List<ImageFileResult> AuthorImages(Author author);
        List<ImageFileResult> BookImages(Author author, BookFile bookFile);
    }
}
