namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ImageResource
    {
        public string CoverType { get; set; }
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}