using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedBookInfo
    {
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public AuthorTitleInfo AuthorTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public string ReleaseDate { get; set; }
        public bool Discography { get; set; }
        public int DiscographyStart { get; set; }
        public int DiscographyEnd { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string ReleaseVersion { get; set; }

        public override string ToString()
        {
            string bookString = "[Unknown Book]";

            if (BookTitle != null)
            {
                bookString = string.Format("{0}", BookTitle);
            }

            return string.Format("{0} - {1} {2}", AuthorName, bookString, Quality);
        }
    }
}
