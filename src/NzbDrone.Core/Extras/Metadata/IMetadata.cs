using System.Collections.Generic;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Artist artist, TrackFile trackFile, MetadataFile metadataFile);
        string GetFilenameAfterMove(Artist artist, string albumPath, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Artist artist, string path);
        MetadataFileResult ArtistMetadata(Artist artist);
        MetadataFileResult AlbumMetadata(Artist artist, Album album, string albumPath);
        MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile);
        List<ImageFileResult> ArtistImages(Artist artist);
        List<ImageFileResult> AlbumImages(Artist artist, Album album, string albumPath);
        List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile);
    }
}
