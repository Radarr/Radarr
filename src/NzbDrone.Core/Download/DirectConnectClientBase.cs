using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class DirectConnectClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected DirectConnectClientBase(
            IConfigService configService,
            INamingConfigService namingConfigService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger)
            : base(configService, namingConfigService, diskProvider, remotePathMappingService, logger)
        {
        }

        public override DownloadProtocol Protocol => DownloadProtocol.DirectConnect;

        public override string Download(RemoteMovie remoteMovie)
        {
            var id = remoteMovie.Release.DownloadUrl;
            _logger.Info("Adding report [{0}] to the queue.", remoteMovie.Release.Title);
            return AddFromId(id, remoteMovie.Release.Title);
        }

        protected abstract string AddFromId(string id, string title);
    }
}
