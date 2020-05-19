using System;
using System.Net;
using MonoTorrent;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class TorrentClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        protected readonly ITorrentFileInfoReader _torrentFileInfoReader;

        protected TorrentClientBase(ITorrentFileInfoReader torrentFileInfoReader,
                                    IHttpClient httpClient,
                                    IConfigService configService,
                                    IDiskProvider diskProvider,
                                    IRemotePathMappingService remotePathMappingService,
                                    Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
            _httpClient = httpClient;
            _torrentFileInfoReader = torrentFileInfoReader;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;

        public virtual bool PreferTorrentFile => false;

        protected abstract string AddFromMagnetLink(RemoteBook remoteBook, string hash, string magnetLink);
        protected abstract string AddFromTorrentFile(RemoteBook remoteBook, string hash, string filename, byte[] fileContent);

        public override string Download(RemoteBook remoteBook)
        {
            var torrentInfo = remoteBook.Release as TorrentInfo;

            string magnetUrl = null;
            string torrentUrl = null;

            if (remoteBook.Release.DownloadUrl.IsNotNullOrWhiteSpace() && remoteBook.Release.DownloadUrl.StartsWith("magnet:"))
            {
                magnetUrl = remoteBook.Release.DownloadUrl;
            }
            else
            {
                torrentUrl = remoteBook.Release.DownloadUrl;
            }

            if (torrentInfo != null && !torrentInfo.MagnetUrl.IsNullOrWhiteSpace())
            {
                magnetUrl = torrentInfo.MagnetUrl;
            }

            if (PreferTorrentFile)
            {
                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromWebUrl(remoteBook, torrentUrl);
                    }
                    catch (Exception ex)
                    {
                        if (!magnetUrl.IsNullOrWhiteSpace())
                        {
                            throw;
                        }

                        _logger.Debug("Torrent download failed, trying magnet. ({0})", ex.Message);
                    }
                }

                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteBook, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        throw new ReleaseDownloadException(remoteBook.Release, "Magnet not supported by download client. ({0})", ex.Message);
                    }
                }
            }
            else
            {
                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteBook, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        if (torrentUrl.IsNullOrWhiteSpace())
                        {
                            throw new ReleaseDownloadException(remoteBook.Release, "Magnet not supported by download client. ({0})", ex.Message);
                        }

                        _logger.Debug("Magnet not supported by download client, trying torrent. ({0})", ex.Message);
                    }
                }

                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    return DownloadFromWebUrl(remoteBook, torrentUrl);
                }
            }

            return null;
        }

        private string DownloadFromWebUrl(RemoteBook remoteBook, string torrentUrl)
        {
            byte[] torrentFile = null;

            try
            {
                var request = new HttpRequest(torrentUrl);
                request.Headers.Accept = "application/x-bittorrent";
                request.AllowAutoRedirect = false;

                var response = _httpClient.Get(request);

                if (response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther)
                {
                    var locationHeader = response.Headers.GetSingleValue("Location");

                    _logger.Trace("Torrent request is being redirected to: {0}", locationHeader);

                    if (locationHeader != null)
                    {
                        if (locationHeader.StartsWith("magnet:"))
                        {
                            return DownloadFromMagnetUrl(remoteBook, locationHeader);
                        }

                        return DownloadFromWebUrl(remoteBook, locationHeader);
                    }

                    throw new WebException("Remote website tried to redirect without providing a location.");
                }

                torrentFile = response.ResponseData;

                _logger.Debug("Downloading torrent for release '{0}' finished ({1} bytes from {2})", remoteBook.Release.Title, torrentFile.Length, torrentUrl);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Error(ex, "Downloading torrent file for book '{0}' failed since it no longer exists ({1})", remoteBook.Release.Title, torrentUrl);
                    throw new ReleaseUnavailableException(remoteBook.Release, "Downloading torrent failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", torrentUrl);
                }
                else
                {
                    _logger.Error(ex, "Downloading torrent file for release '{0}' failed ({1})", remoteBook.Release.Title, torrentUrl);
                }

                throw new ReleaseDownloadException(remoteBook.Release, "Downloading torrent failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading torrent file for release '{0}' failed ({1})", remoteBook.Release.Title, torrentUrl);

                throw new ReleaseDownloadException(remoteBook.Release, "Downloading torrent failed", ex);
            }

            var filename = string.Format("{0}.torrent", FileNameBuilder.CleanFileName(remoteBook.Release.Title));
            var hash = _torrentFileInfoReader.GetHashFromTorrentFile(torrentFile);
            var actualHash = AddFromTorrentFile(remoteBook, hash, filename, torrentFile);

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Readarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteBook.Release.DownloadUrl);
            }

            return actualHash;
        }

        private string DownloadFromMagnetUrl(RemoteBook remoteBook, string magnetUrl)
        {
            string hash = null;
            string actualHash = null;

            try
            {
                hash = InfoHash.FromMagnetLink(magnetUrl).ToHex();
            }
            catch (FormatException ex)
            {
                _logger.Error(ex, "Failed to parse magnetlink for release '{0}': '{1}'", remoteBook.Release.Title, magnetUrl);

                return null;
            }

            if (hash != null)
            {
                actualHash = AddFromMagnetLink(remoteBook, hash, magnetUrl);
            }

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Readarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteBook.Release.DownloadUrl);
            }

            return actualHash;
        }
    }
}
