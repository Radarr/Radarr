using System;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Audiobooks
{
    public class Audiobook : MediaItem
    {
        public Audiobook()
        {
            MediaType = MediaType.Audiobook;
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignAudiobookId { get; set; }
        public string Isbn { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }

        public string Narrator { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsAbridged { get; set; }

        public int? SeriesPosition { get; set; }

        public int? BookId { get; set; }

        public override string GetTitle() => Title;
        public override int GetYear() => ReleaseDate?.Year ?? 0;

        public override string ToString()
        {
            var narratorInfo = string.IsNullOrEmpty(Narrator) ? "" : $" - Narrated by {Narrator}";
            return $"{Title} ({ReleaseDate?.Year}){narratorInfo}";
        }
    }
}
