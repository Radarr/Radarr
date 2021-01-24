using NzbDrone.Core.Organizer;

namespace Readarr.Api.V1.Config
{
    public class NamingExampleResource
    {
        public string SingleBookExample { get; set; }
        public string AuthorFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameBooks = model.RenameBooks,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                StandardBookFormat = model.StandardBookFormat,
                AuthorFolderFormat = model.AuthorFolderFormat
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeAuthorName = basicNamingConfig.IncludeAuthorName;
            resource.IncludeBookTitle = basicNamingConfig.IncludeBookTitle;
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

                RenameBooks = resource.RenameBooks,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                StandardBookFormat = resource.StandardBookFormat,
                AuthorFolderFormat = resource.AuthorFolderFormat,
            };
        }
    }
}
