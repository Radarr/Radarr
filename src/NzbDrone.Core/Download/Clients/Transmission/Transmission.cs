using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class Transmission : TransmissionBase
    {
        public Transmission(ITransmissionProxy proxy,
                            ITorrentFileInfoReader torrentFileInfoReader,
                            IHttpClient httpClient,
                            IConfigService configService,
                            IDiskProvider diskProvider,
                            IRemotePathMappingService remotePathMappingService,
                            IBlocklistService blocklistService,
                            Logger logger)
            : base(proxy, torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, blocklistService, logger)
        {
        }

        protected override ValidationFailure ValidateVersion()
        {
            var version = _proxy.GetClientVersion(Settings);

            _logger.Debug("Transmission version information: {0}", version);

            if (version < new Version(2, 40))
            {
                return new ValidationFailure(string.Empty, "Transmission version not supported, should be 2.40 or higher.");
            }

            return null;
        }

        public override string Name => "Transmission";
    }
}
