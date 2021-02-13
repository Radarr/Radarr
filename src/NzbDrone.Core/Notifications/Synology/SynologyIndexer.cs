using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Synology
{
    public class SynologyIndexer : NotificationBase<SynologyIndexerSettings>
    {
        private readonly ISynologyIndexerProxy _indexerProxy;

        public SynologyIndexer(ISynologyIndexerProxy indexerProxy)
        {
            _indexerProxy = indexerProxy;
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
                return new ValidationFailure(null, "Must be a Synology");
            }

            if (!_indexerProxy.Test())
            {
                return new ValidationFailure(null, "Not a Synology or synoindex not available");
            }

            return null;
        }
    }
}
