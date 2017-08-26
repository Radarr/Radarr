using NzbDrone.Api.REST;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Api.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameTracks { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardTrackFormat { get; set; }
        public string ArtistFolderFormat { get; set; }
        public string AlbumFolderFormat { get; set; }
        public bool IncludeArtistName { get; set; }
        public bool IncludeAlbumTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
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
                ArtistFolderFormat = model.ArtistFolderFormat,
                AlbumFolderFormat = model.AlbumFolderFormat
                //IncludeSeriesTitle
                //IncludeEpisodeTitle
                //IncludeQuality
                //ReplaceSpaces
                //Separator
                //NumberStyle
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
                ArtistFolderFormat = resource.ArtistFolderFormat,
                AlbumFolderFormat = resource.AlbumFolderFormat
            };
        }
    }
}
