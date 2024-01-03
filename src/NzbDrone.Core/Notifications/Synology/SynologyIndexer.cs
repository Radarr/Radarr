using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Synology
{
    public class SynologyIndexer : NotificationBase<SynologyIndexerSettings>
    {
        private readonly ISynologyIndexerProxy _indexerProxy;
        private readonly ILocalizationService _localizationService;

        public SynologyIndexer(ISynologyIndexerProxy indexerProxy, ILocalizationService localizationService)
        {
            _indexerProxy = indexerProxy;
            _localizationService = localizationService;
        }

        public override string Link => "https://www.synology.com";
        public override string Name => "Synology Indexer";

        public override void OnDownload(DownloadMessage message)
        {
            if (Settings.UpdateLibrary)
            {
                foreach (var oldFile in message.OldMovieFiles)
                {
                    var fullPath = Path.Combine(message.Movie.Path, oldFile.RelativePath);

                    _indexerProxy.DeleteFile(fullPath);
                }

                {
                    var fullPath = Path.Combine(message.Movie.Path, message.MovieFile.RelativePath);

                    _indexerProxy.AddFile(fullPath);
                }
            }
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            if (Settings.UpdateLibrary)
            {
                _indexerProxy.UpdateFolder(movie.Path);
            }
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            if (Settings.UpdateLibrary)
            {
                var fullPath = Path.Combine(deleteMessage.Movie.Path, deleteMessage.MovieFile.RelativePath);
                _indexerProxy.DeleteFile(fullPath);
            }
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                if (Settings.UpdateLibrary)
                {
                    _indexerProxy.DeleteFolder(deleteMessage.Movie.Path);
                }
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestConnection());

            return new ValidationResult(failures);
        }

        protected virtual ValidationFailure TestConnection()
        {
            if (!OsInfo.IsLinux)
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsSynologyValidationInvalidOs"));
            }

            if (!_indexerProxy.Test())
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsSynologyValidationTestFailed"));
            }

            return null;
        }
    }
}
