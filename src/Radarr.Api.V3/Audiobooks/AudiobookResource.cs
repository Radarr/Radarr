using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Audiobooks;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Audiobooks
{
    public class AudiobookResource : RestResource
    {
        public AudiobookResource()
        {
            Monitored = true;
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

        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public int? AuthorId { get; set; }
        public int? SeriesId { get; set; }
        public int? SeriesPosition { get; set; }
        public int? BookId { get; set; }
    }

    public static class AudiobookResourceMapper
    {
        public static AudiobookResource ToResource(this Audiobook model)
        {
            if (model == null)
            {
                return null;
            }

            return new AudiobookResource
            {
                Id = model.Id,
                Title = model.Title,
                SortTitle = model.SortTitle,
                Description = model.Description,
                ForeignAudiobookId = model.ForeignAudiobookId,
                Isbn = model.Isbn,
                Isbn13 = model.Isbn13,
                Asin = model.Asin,
                ReleaseDate = model.ReleaseDate,
                Publisher = model.Publisher,
                Language = model.Language,
                Narrator = model.Narrator,
                DurationMinutes = model.DurationMinutes,
                IsAbridged = model.IsAbridged,
                Monitored = model.Monitored,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                Added = model.Added,
                Tags = model.Tags,
                LastSearchTime = model.LastSearchTime,
                AuthorId = model.AuthorId,
                SeriesId = model.SeriesId,
                SeriesPosition = model.SeriesPosition,
                BookId = model.BookId
            };
        }

        public static Audiobook ToModel(this AudiobookResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Audiobook
            {
                Id = resource.Id,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                Description = resource.Description,
                ForeignAudiobookId = resource.ForeignAudiobookId,
                Isbn = resource.Isbn,
                Isbn13 = resource.Isbn13,
                Asin = resource.Asin,
                ReleaseDate = resource.ReleaseDate,
                Publisher = resource.Publisher,
                Language = resource.Language,
                Narrator = resource.Narrator,
                DurationMinutes = resource.DurationMinutes,
                IsAbridged = resource.IsAbridged,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                Tags = resource.Tags ?? new HashSet<int>(),
                AuthorId = resource.AuthorId,
                SeriesId = resource.SeriesId,
                SeriesPosition = resource.SeriesPosition,
                BookId = resource.BookId
            };
        }

        public static Audiobook ToModel(this AudiobookResource resource, Audiobook audiobook)
        {
            var updatedAudiobook = resource.ToModel();

            audiobook.Title = updatedAudiobook.Title;
            audiobook.SortTitle = updatedAudiobook.SortTitle;
            audiobook.Description = updatedAudiobook.Description;
            audiobook.ForeignAudiobookId = updatedAudiobook.ForeignAudiobookId;
            audiobook.Isbn = updatedAudiobook.Isbn;
            audiobook.Isbn13 = updatedAudiobook.Isbn13;
            audiobook.Asin = updatedAudiobook.Asin;
            audiobook.ReleaseDate = updatedAudiobook.ReleaseDate;
            audiobook.Publisher = updatedAudiobook.Publisher;
            audiobook.Language = updatedAudiobook.Language;
            audiobook.Narrator = updatedAudiobook.Narrator;
            audiobook.DurationMinutes = updatedAudiobook.DurationMinutes;
            audiobook.IsAbridged = updatedAudiobook.IsAbridged;
            audiobook.Monitored = updatedAudiobook.Monitored;
            audiobook.QualityProfileId = updatedAudiobook.QualityProfileId;
            audiobook.Path = updatedAudiobook.Path;
            audiobook.RootFolderPath = updatedAudiobook.RootFolderPath;
            audiobook.Tags = updatedAudiobook.Tags;
            audiobook.AuthorId = updatedAudiobook.AuthorId;
            audiobook.SeriesId = updatedAudiobook.SeriesId;
            audiobook.SeriesPosition = updatedAudiobook.SeriesPosition;
            audiobook.BookId = updatedAudiobook.BookId;

            return audiobook;
        }

        public static List<AudiobookResource> ToResource(this IEnumerable<Audiobook> audiobooks)
        {
            return audiobooks.Select(ToResource).ToList();
        }

        public static List<Audiobook> ToModel(this IEnumerable<AudiobookResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
