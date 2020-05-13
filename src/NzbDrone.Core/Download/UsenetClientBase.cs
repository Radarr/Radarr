using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class UsenetClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        private readonly IValidateNzbs _nzbValidationService;

        protected UsenetClientBase(IHttpClient httpClient,
                                   IConfigService configService,
                                   IDiskProvider diskProvider,
                                   IRemotePathMappingService remotePathMappingService,
                                   IValidateNzbs nzbValidationService,
                                   Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
            _httpClient = httpClient;
            _nzbValidationService = nzbValidationService;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        protected abstract string AddFromNzbFile(RemoteBook remoteAlbum, string filename, byte[] fileContent);

        public override string Download(RemoteBook remoteAlbum)
        {
            var url = remoteAlbum.Release.DownloadUrl;
            var filename = FileNameBuilder.CleanFileName(remoteAlbum.Release.Title) + ".nzb";

            byte[] nzbData;

            try
            {
                var nzbDataRequest = new HttpRequest(url);

                // TODO: Look into moving download request handling to indexer
                if (remoteAlbum.Release.BasicAuthString.IsNotNullOrWhiteSpace())
                {
                    nzbDataRequest.Headers.Set("Authorization", "Basic " + remoteAlbum.Release.BasicAuthString);
                }

                nzbData = _httpClient.Get(nzbDataRequest).ResponseData;

                _logger.Debug("Downloaded nzb for release '{0}' finished ({1} bytes from {2})", remoteAlbum.Release.Title, nzbData.Length, url);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Error(ex, "Downloading nzb file for book '{0}' failed since it no longer exists ({1})", remoteAlbum.Release.Title, url);
                    throw new ReleaseUnavailableException(remoteAlbum.Release, "Downloading torrent failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", url);
                }
                else
                {
                    _logger.Error(ex, "Downloading nzb for release '{0}' failed ({1})", remoteAlbum.Release.Title, url);
                }

                throw new ReleaseDownloadException(remoteAlbum.Release, "Downloading nzb failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading nzb for release '{0}' failed ({1})", remoteAlbum.Release.Title, url);

                throw new ReleaseDownloadException(remoteAlbum.Release, "Downloading nzb failed", ex);
            }

            _nzbValidationService.Validate(filename, nzbData);

            _logger.Info("Adding report [{0}] to the queue.", remoteAlbum.Release.Title);
            return AddFromNzbFile(remoteAlbum, filename, nzbData);
        }
    }
}
