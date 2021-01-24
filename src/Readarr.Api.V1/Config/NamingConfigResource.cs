using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameBooks { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public string StandardBookFormat { get; set; }
        public string AuthorFolderFormat { get; set; }
        public bool IncludeAuthorName { get; set; }
        public bool IncludeBookTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}
