namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class RatingResource
    {
        public int Count { get; set; }
        public decimal Value { get; set; }
        public string Origin { get; set; }
        public string Type { get; set; }
    }
}
