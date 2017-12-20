namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ArtistResponse
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public ArtistResult Result { get; set; }
    }
}
