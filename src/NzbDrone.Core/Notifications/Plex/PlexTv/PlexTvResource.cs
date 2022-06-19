namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public class PlexTvResource
    {
        public string Name { get; set; }
        public string Product { get; set; }
        public string Platform { get; set; }
        public string ClientIdentifier { get; set; }
        public string Provides { get; set; }
        public bool Owned { get; set; }
        public bool Home { get; set; }
    }
}
