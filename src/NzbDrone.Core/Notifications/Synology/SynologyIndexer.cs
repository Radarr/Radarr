using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

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


        public override void OnDownload(TrackDownloadMessage message)
        {
            if (Settings.UpdateLibrary)
            {
                foreach (var oldFile in message.OldFiles)
                {
                    var fullPath = Path.Combine(message.Artist.Path, oldFile.RelativePath);

                    _indexerProxy.DeleteFile(fullPath);
                }

                {
                    var fullPath = Path.Combine(message.Artist.Path, message.TrackFile.RelativePath);

                    _indexerProxy.AddFile(fullPath);
                }
            }
        }

        public override void OnRename(Artist artist)
        {
            if (Settings.UpdateLibrary)
            {
                _indexerProxy.UpdateFolder(artist.Path);
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
