using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int artistId, IEnumerable<MediaCover> covers, int? albumId = null);
        string GetCoverPath(int artistId, MediaCoverTypes mediaCoverTypes, int? height = null, int? albumId = null);
    }

    public class MediaCoverService :
        IHandleAsync<ArtistUpdatedEvent>,
        IHandleAsync<ArtistDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IImageResizer _resizer;
        private readonly IAlbumService _albumService;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        public MediaCoverService(IImageResizer resizer,
                                 IAlbumService albumService,     
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _resizer = resizer;
            _albumService = albumService;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int artistId, MediaCoverTypes coverTypes, int? height = null, int? albumId = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            if (albumId.HasValue)
            {
                return Path.Combine(GetAlbumCoverPath(artistId, albumId.Value), coverTypes.ToString().ToLower() + heightSuffix + ".jpg");
            }

            return Path.Combine(GetArtistCoverPath(artistId), coverTypes.ToString().ToLower() + heightSuffix + ".jpg");
        }

        public void ConvertToLocalUrls(int artistId, IEnumerable<MediaCover> covers, int? albumId = null)
        {
            foreach (var mediaCover in covers)
            {
                var filePath = GetCoverPath(artistId, mediaCover.CoverType, null, albumId);

                if (albumId.HasValue)
                {
                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + artistId + "/" + albumId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";
                }
                else
                {
                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + artistId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";
                }

                if (_diskProvider.FileExists(filePath))
                {
                    var lastWrite = _diskProvider.FileGetLastWrite(filePath);
                    mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                }
            }
        }

        private string GetArtistCoverPath(int artistId)
        {
            return Path.Combine(_coverRootFolder, artistId.ToString());
        }

        private string GetAlbumCoverPath(int artistId, int albumId)
        {
            return Path.Combine(_coverRootFolder, artistId.ToString(), albumId.ToString());
        }

        private void EnsureCovers(Artist artist)
        {
            foreach (var cover in artist.Metadata.Value.Images)
            {
                var fileName = GetCoverPath(artist.Id, cover.CoverType);
                var alreadyExists = false;
                
                try
                {
                    var lastModifiedServer = GetCoverModifiedDate(cover.Url);

                    alreadyExists = _coverExistsSpecification.AlreadyExists(lastModifiedServer, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(artist, cover, lastModifiedServer);
                    }
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", artist, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", artist);
                }

                EnsureResizedCovers(artist, cover, !alreadyExists);
            }
        }

        //TODO Decide if we want to cache album art local
        //private void EnsureAlbumCovers(Album album)
        //{
        //    foreach (var cover in album.Images)
        //    {
        //        var fileName = GetCoverPath(album.ArtistId, cover.CoverType, null,  album.Id);
        //        var alreadyExists = false;
        //        try
        //        {
        //            alreadyExists = _coverExistsSpecification.AlreadyExists(cover.Url, fileName);
        //            if (!alreadyExists)
        //            {
        //                DownloadAlbumCover(album, cover);
        //            }
        //        }
        //        catch (WebException e)
        //        {
        //            _logger.Warn("Couldn't download media cover for {0}. {1}", album, e.Message);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.Error(e, "Couldn't download media cover for {0}", album);
        //        }

        //        EnsureResizedCovers(album.Artist, cover, !alreadyExists, album);
        //    }
        //}

        private void DownloadCover(Artist artist, MediaCover cover, DateTime lastModified)
        {
            var fileName = GetCoverPath(artist.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, artist, cover.Url);
            _httpClient.DownloadFile(cover.Url, fileName);

            try
            {
                _diskProvider.FileSetLastWriteTime(fileName, lastModified);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to set modified date for {0} image for artist {1}", cover.CoverType, artist);
            }
        }

        //private void DownloadAlbumCover(Album album, MediaCover cover)
        //{
        //    var fileName = GetCoverPath(album.ArtistId, cover.CoverType, null, album.Id);

        //    _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, album, cover.Url);
        //    _httpClient.DownloadFile(cover.Url, fileName);
        //}

        private void EnsureResizedCovers(Artist artist, MediaCover cover, bool forceResize, Album album = null)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Cover:
                case MediaCoverTypes.Disc:
                case MediaCoverTypes.Logo:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }
            

            if (album == null)
            {
                foreach (var height in heights)
                {
                    var mainFileName = GetCoverPath(artist.Id, cover.CoverType);
                    var resizeFileName = GetCoverPath(artist.Id, cover.CoverType, height);

                    if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                    {
                        _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, artist);

                        try
                        {
                            _resizer.Resize(mainFileName, resizeFileName, height);
                        }
                        catch
                        {
                            _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, artist);
                        }
                    }
                }
            }
            else
            {
                foreach (var height in heights)
                {
                    var mainFileName = GetCoverPath(album.ArtistId, cover.CoverType, null, album.Id);
                    var resizeFileName = GetCoverPath(album.ArtistId, cover.CoverType, height, album.Id);

                    if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                    {
                        _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, artist);

                        try
                        {
                            _resizer.Resize(mainFileName, resizeFileName, height);
                        }
                        catch
                        {
                            _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, album);
                        }
                    }
                }
            }
        }

        public void HandleAsync(ArtistUpdatedEvent message)
        {
            EnsureCovers(message.Artist);

            //Turn off for now, not using album images

            //var albums = _albumService.GetAlbumsByArtist(message.Artist.Id);
            //foreach (Album album in albums)
            //{
            //    EnsureAlbumCovers(album);
            //}
            
            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Artist));
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            var path = GetArtistCoverPath(message.Artist.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }

        private DateTime GetCoverModifiedDate(string url)
        {
            var lastModifiedServer = DateTime.Now;

            var headers = _httpClient.Head(new HttpRequest(url)).Headers;

            if (headers.LastModified.HasValue)
            {
                lastModifiedServer = headers.LastModified.Value;
            }

            return lastModifiedServer;
        }
    }
}
