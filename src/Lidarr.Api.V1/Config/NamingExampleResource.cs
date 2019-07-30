using NzbDrone.Core.Organizer;

namespace Lidarr.Api.V1.Config
{
    public class NamingExampleResource
    {
        public string SingleTrackExample { get; set; }
        public string MultiDiscTrackExample { get; set; }
        public string ArtistFolderExample { get; set; }
        public string AlbumFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameTracks = model.RenameTracks,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                StandardTrackFormat = model.StandardTrackFormat,
                MultiDiscTrackFormat = model.MultiDiscTrackFormat,
                ArtistFolderFormat = model.ArtistFolderFormat,
                AlbumFolderFormat = model.AlbumFolderFormat
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeArtistName = basicNamingConfig.IncludeArtistName;
            resource.IncludeAlbumTitle = basicNamingConfig.IncludeAlbumTitle;
            resource.IncludeQuality = basicNamingConfig.IncludeQuality;
            resource.ReplaceSpaces = basicNamingConfig.ReplaceSpaces;
            resource.Separator = basicNamingConfig.Separator;
            resource.NumberStyle = basicNamingConfig.NumberStyle;
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameTracks = resource.RenameTracks,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                StandardTrackFormat = resource.StandardTrackFormat,
                MultiDiscTrackFormat = resource.MultiDiscTrackFormat,

                ArtistFolderFormat = resource.ArtistFolderFormat,
                AlbumFolderFormat = resource.AlbumFolderFormat
            };
        }
    }
}
