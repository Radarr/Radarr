using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class HttpApiProvider : IApiProvider
    {
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public HttpApiProvider(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public bool CanHandle(XbmcVersion version)
        {
            return version < new XbmcVersion(5);
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            var notification = string.Format("Notification({0},{1},{2},{3})", title, message, settings.DisplayTime * 1000, "https://raw.github.com/Lidarr/Lidarr/develop/Logo/64.png");
            var command = BuildExecBuiltInCommand(notification);

            SendCommand(settings, command);
        }

        public void Update(XbmcSettings settings, Artist artist)
        {
            if (!settings.AlwaysUpdate)
            {
                _logger.Debug("Determining if there are any active players on XBMC host: {0}", settings.Address);
                var activePlayers = GetActivePlayers(settings);

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
            const string cleanMusicLibrary = "CleanLibrary(music)";
            var command = BuildExecBuiltInCommand(cleanMusicLibrary);

            SendCommand(settings, command);
        }

        internal List<ActivePlayer> GetActivePlayers(XbmcSettings settings)
        {
            try
            {
                var result = new List<ActivePlayer>();
                var response = SendCommand(settings, "getcurrentlyplaying");

                if (response.Contains("<li>Filename:[Nothing Playing]")) return new List<ActivePlayer>();
                if (response.Contains("<li>Type:Audio")) result.Add(new ActivePlayer(1, "audio"));

                return result;
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }

            return new List<ActivePlayer>();
        }
        
        internal string GetArtistPath(XbmcSettings settings, Artist artist)
        {
            var query =
                string.Format(
                    "select path.strPath from path, artist, artistlinkpath where artist.c12 = {0} and artistlinkpath.idArtist = artist.idArtist and artistlinkpath.idPath = path.idPath",
                    artist.Metadata.Value.ForeignArtistId);
            var command = string.Format("QueryMusicDatabase({0})", query);

            const string setResponseCommand =
                "SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            const string resetResponseCommand = "SetResponseFormat()";

            SendCommand(settings, setResponseCommand);
            var response = SendCommand(settings, command);
            SendCommand(settings, resetResponseCommand);

            if (string.IsNullOrEmpty(response))
                return string.Empty;

            var xDoc = XDocument.Load(new StringReader(response.Replace("&", "&amp;")));
            var xml = xDoc.Descendants("xml").Select(x => x).FirstOrDefault();

            if (xml == null)
                return null;

            var field = xml.Descendants("field").FirstOrDefault();

            if (field == null)
                return null;

            return field.Value;
        }

        internal bool CheckForError(string response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.Debug("Invalid response from XBMC, the response is not valid JSON");
                return true;
            }

            var errorIndex = response.IndexOf("Error", StringComparison.InvariantCultureIgnoreCase);

            if (errorIndex > -1)
            {
                var errorMessage = response.Substring(errorIndex + 6);
                errorMessage = errorMessage.Substring(0, errorMessage.IndexOfAny(new char[] { '<', ';' }));

                _logger.Debug("Error found in response: {0}", errorMessage);
                return true;
            }

            return false;
        }

        private void UpdateLibrary(XbmcSettings settings, Artist artist)
        {
            try
            {
                _logger.Debug("Sending Update DB Request to XBMC Host: {0}", settings.Address);
                var xbmcArtistPath = GetArtistPath(settings, artist);

                //If the path is found update it, else update the whole library
                if (!string.IsNullOrEmpty(xbmcArtistPath))
                {
                    _logger.Debug("Updating artist [{0}] on XBMC host: {1}", artist, settings.Address);
                    var command = BuildExecBuiltInCommand(string.Format("UpdateLibrary(music,{0})", xbmcArtistPath));
                    SendCommand(settings, command);
                }

                else
                {
                    //Update the entire library
                    _logger.Debug("Artist [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", artist, settings.Address);
                    var command = BuildExecBuiltInCommand("UpdateLibrary(music)");
                    SendCommand(settings, command);
                }
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }
        }

        private string SendCommand(XbmcSettings settings, string command)
        {
            var url = string.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", settings.Address, command);

            if (!string.IsNullOrEmpty(settings.Username))
            {
                return _httpProvider.DownloadString(url, settings.Username, settings.Password);
            }

            return _httpProvider.DownloadString(url);
        }

        private string BuildExecBuiltInCommand(string command)
        {
            return string.Format("ExecBuiltIn({0})", command);
        }
    }
}
