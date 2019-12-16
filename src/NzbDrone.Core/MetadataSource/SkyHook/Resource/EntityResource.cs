namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class EntityResource
    {
        public int Score { get; set; }
        public ArtistResource Artist { get; set; }
        public AlbumResource Album { get; set; }

    }
}
