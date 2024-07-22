using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using Radarr.Http.REST;

namespace Radarr.Api.V3.ExtraFiles
{
    public class ExtraFileResource : RestResource
    {
        public int MovieId { get; set; }
        public int? MovieFileId { get; set; }
        public string RelativePath { get; set; }
        public string Extension { get; set; }
        public List<string> LanguageTags { get; set; }
        public string Title { get; set; }
        public ExtraFileType Type { get; set; }
    }

    public static class ExtraFileResourceMapper
    {
        public static ExtraFileResource ToResource(this MetadataFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new ExtraFileResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                MovieFileId = model.MovieFileId,
                RelativePath = model.RelativePath,
                Extension = model.Extension,
                Type = ExtraFileType.Metadata
            };
        }

        public static ExtraFileResource ToResource(this SubtitleFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new ExtraFileResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                MovieFileId = model.MovieFileId,
                RelativePath = model.RelativePath,
                Extension = model.Extension,
                LanguageTags = model.LanguageTags,
                Title = model.Title,
                Type = ExtraFileType.Subtitle
            };
        }

        public static ExtraFileResource ToResource(this OtherExtraFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new ExtraFileResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                MovieFileId = model.MovieFileId,
                RelativePath = model.RelativePath,
                Extension = model.Extension,
                Type = ExtraFileType.Other
            };
        }

        public static List<ExtraFileResource> ToResource(this IEnumerable<SubtitleFile> movies)
        {
            return movies.Select(ToResource).ToList();
        }

        public static List<ExtraFileResource> ToResource(this IEnumerable<MetadataFile> movies)
        {
            return movies.Select(ToResource).ToList();
        }

        public static List<ExtraFileResource> ToResource(this IEnumerable<OtherExtraFile> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
