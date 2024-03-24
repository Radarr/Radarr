using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateSubtitleInfo : IAggregateLocalMovie
    {
        public int Order => 2;

        private readonly Logger _logger;

        public AggregateSubtitleInfo(Logger logger)
        {
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var path = localMovie.Path;
            var isSubtitleFile = SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path));

            if (!isSubtitleFile)
            {
                return localMovie;
            }

            localMovie.SubtitleInfo = CleanSubtitleTitleInfo(localMovie.Movie.MovieFile, path, localMovie.FileNameBeforeRename);

            return localMovie;
        }

        public SubtitleTitleInfo CleanSubtitleTitleInfo(MovieFile movieFile, string path, string fileNameBeforeRename)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(path);

            var movieFileTitle = Path.GetFileNameWithoutExtension(fileNameBeforeRename ?? movieFile.RelativePath);
            var originalMovieFileTitle = Path.GetFileNameWithoutExtension(movieFile.OriginalFilePath) ?? string.Empty;

            if (subtitleTitleInfo.TitleFirst && (movieFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase) || originalMovieFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Debug("Subtitle title '{0}' is in movie file title '{1}'. Removing from subtitle title.", subtitleTitleInfo.RawTitle, movieFileTitle);

                subtitleTitleInfo = LanguageParser.ParseBasicSubtitle(path);
            }

            var cleanedTags = subtitleTitleInfo.LanguageTags.Where(t => !movieFileTitle.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();

            if (cleanedTags.Count != subtitleTitleInfo.LanguageTags.Count)
            {
                _logger.Debug("Removed language tags '{0}' from subtitle title '{1}'.", string.Join(", ", subtitleTitleInfo.LanguageTags.Except(cleanedTags)), subtitleTitleInfo.RawTitle);
                subtitleTitleInfo.LanguageTags = cleanedTags;
            }

            return subtitleTitleInfo;
        }
    }
}
