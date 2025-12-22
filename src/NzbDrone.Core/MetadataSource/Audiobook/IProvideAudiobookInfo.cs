using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Audiobook
{
    public interface IProvideAudiobookInfo : IProvideMediaInfo<AudiobookMetadata>, ISearchableMediaProvider<AudiobookMetadata>
    {
        AudiobookMetadata GetByIsbn(string isbn);
        AudiobookMetadata GetByAsin(string asin);
        List<AudiobookMetadata> GetByNarrator(string narratorName);
        List<AudiobookMetadata> GetByAuthor(string authorName);
    }

    public class AudiobookMetadata
    {
        public string ForeignAudiobookId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string Isbn { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public string Narrator { get; set; }
        public List<string> Narrators { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsAbridged { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Genres { get; set; }
        public string CoverUrl { get; set; }
        public double? Rating { get; set; }
        public int? RatingsCount { get; set; }
        public int? BookId { get; set; }
    }
}
