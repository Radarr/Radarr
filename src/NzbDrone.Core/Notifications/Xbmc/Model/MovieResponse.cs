namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class MovieResponse
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public MovieResult Result { get; set; }
    }
}
