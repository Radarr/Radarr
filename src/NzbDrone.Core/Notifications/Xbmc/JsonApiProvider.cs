using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class JsonApiProvider : IApiProvider
    {
        private readonly IXbmcJsonApiProxy _proxy;
        private readonly Logger _logger;

        public JsonApiProvider(IXbmcJsonApiProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public bool CanHandle(XbmcVersion version)
        {
            return version >= new XbmcVersion(5);
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void Update(XbmcSettings settings, Artist artist)
        {
            if (!settings.AlwaysUpdate)
            {
                _logger.Debug("Determining if there are any active players on XBMC host: {0}", settings.Address);
                var activePlayers = _proxy.GetActivePlayers(settings);

                if (activePlayers.Any(a => a.Type.Equals("audio")))
                {
                    _logger.Debug("Audio is currently playing, skipping library update");
                    return;
                }
            }

            UpdateLibrary(settings, artist);
        }
        
        public void Clean(XbmcSettings settings)
        {
            _proxy.CleanLibrary(settings);
        }

        public List<ActivePlayer> GetActivePlayers(XbmcSettings settings)
        {
            return _proxy.GetActivePlayers(settings); 
        }

        public string GetArtistPath(XbmcSettings settings, Artist artist)
        {
            var allArtists = _proxy.GetArtist(settings);

            if (!allArtists.Any())
            {
                _logger.Debug("No Artists returned from XBMC");
                return null;
            }

            var matchingArtist = allArtists.FirstOrDefault(s =>
            {
                var musicBrainzId = s.MusicbrainzArtistId.FirstOrDefault();

                return musicBrainzId == artist.Metadata.Value.ForeignArtistId || s.Label == artist.Name;
            });

            return matchingArtist?.File;
        }

        private void UpdateLibrary(XbmcSettings settings, Artist artist)
        {
            try
            {
                var artistPath = GetArtistPath(settings, artist);

                if (artistPath != null)
                {
                    _logger.Debug("Updating artist {0} (Path: {1}) on XBMC host: {2}", artist, artistPath, settings.Address);
                }

                else
                {
                    _logger.Debug("Artist {0} doesn't exist on XBMC host: {1}, Updating Entire Library", artist,
                                 settings.Address);
                }

                var response = _proxy.UpdateLibrary(settings, artistPath);

                if (!response.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Failed to update library for: {0}", settings.Address);
                }
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }
        }
    }
}
