using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class RefreshArtistService : IExecute<RefreshArtistCommand>
    {
        private readonly IProvideArtistInfo _artistInfo;
        private readonly IArtistService _artistService;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IEventAggregator _eventAggregator;
        //private readonly IDailySeriesService _dailySeriesService;
        private readonly IDiskScanService _diskScanService;
        //private readonly ICheckIfArtistShouldBeRefreshed _checkIfArtistShouldBeRefreshed;
        private readonly Logger _logger;

        public RefreshArtistService(IProvideArtistInfo artistInfo,
                                    IArtistService artistService,
                                    IRefreshTrackService refreshTrackService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    //ICheckIfArtistShouldBeRefreshed checkIfArtistShouldBeRefreshed,
                                    Logger logger)
        {
            _artistInfo = artistInfo;
            _artistService = artistService;
            _refreshTrackService = refreshTrackService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            //_checkIfArtistShouldBeRefreshed = checkIfArtistShouldBeRefreshed;
            _logger = logger;
        }

        private void RefreshArtistInfo(Artist artist)
        {
            _logger.ProgressInfo("Updating Info for {0}", artist.ArtistName);

            Tuple<Artist, List<Track>> tuple;

            try
            {
                tuple = _artistInfo.GetArtistInfo(artist.SpotifyId);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error("Artist '{0}' (SpotifyId {1}) was not found, it may have been removed from Spotify.", artist.ArtistName, artist.SpotifyId);
                return;
            }

            var artistInfo = tuple.Item1;

            if (artist.SpotifyId != artistInfo.SpotifyId)
            {
                _logger.Warn("Artist '{0}' (SpotifyId {1}) was replaced with '{2}' (SpotifyId {3}), because the original was a duplicate.", artist.ArtistName, artist.SpotifyId, artistInfo.ArtistName, artistInfo.SpotifyId);
                artist.SpotifyId = artistInfo.SpotifyId;
            }

            artist.ArtistName = artistInfo.ArtistName;
            artist.ArtistSlug = artistInfo.ArtistSlug;
            artist.Overview = artistInfo.Overview;
            artist.Status = artistInfo.Status;
            artist.CleanTitle = artistInfo.CleanTitle;
            artist.LastInfoSync = DateTime.UtcNow;
            artist.Images = artistInfo.Images;
            //artist.Actors = artistInfo.Actors;
            artist.Genres = artistInfo.Genres;

            try
            {
                artist.Path = new DirectoryInfo(artist.Path).FullName;
                artist.Path = artist.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update artist path for " + artist.Path);
            }

            artist.Albums = UpdateAlbums(artist, artistInfo);

            _artistService.UpdateArtist(artist);
            _refreshTrackService.RefreshTrackInfo(artist, tuple.Item2);

            _logger.Debug("Finished artist refresh for {0}", artist.ArtistName);
            _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
        }

        private List<Album> UpdateAlbums(Artist artist, Artist artistInfo)
        {
            var albums = artistInfo.Albums.DistinctBy(s => s.AlbumId).ToList();

            foreach (var album in albums)
            {
                var existingAlbum = artist.Albums.FirstOrDefault(s => s.AlbumId == album.AlbumId);

                //Todo: Should this should use the previous season's monitored state?
                if (existingAlbum == null)
                {
                    //if (album.SeasonNumber == 0)
                    //{
                    //    album.Monitored = false;
                    //    continue;
                    //}

                    _logger.Debug("New album ({0}) for artist: [{1}] {2}, setting monitored to true", album.Title, artist.SpotifyId, artist.ArtistName);
                    album.Monitored = true;
                }

                else
                {
                    album.Monitored = existingAlbum.Monitored;
                }
            }

            return albums;
        }

        public void Execute(RefreshArtistCommand message)
        {
            _eventAggregator.PublishEvent(new ArtistRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.ArtistId.HasValue)
            {
                var artist = _artistService.GetArtist(message.ArtistId.Value);
                RefreshArtistInfo(artist);
            }
            else
            {
                var allArtists = _artistService.GetAllArtists().OrderBy(c => c.ArtistName).ToList();

                foreach (var artist in allArtists)
                {
                    if (message.Trigger == CommandTrigger.Manual /*|| _checkIfArtistShouldBeRefreshed.ShouldRefresh(artist)*/)
                    {
                        try
                        {
                            RefreshArtistInfo(artist);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", artist);
                        }
                    }

                    else
                    {
                        try
                        {
                            _logger.Info("Skipping refresh of artist: {0}", artist.ArtistName);
                            //TODO: _diskScanService.Scan(artist);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't rescan artist {0}", artist);
                        }
                    }
                }
            }
        }
    }
}
