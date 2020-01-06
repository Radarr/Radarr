namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ImdbResource
    {
        public int v { get; set; }
        public string q { get; set; }
        public MovieResource[] d { get; set; }
    }

    public class MovieResource
    {
        public string l { get; set; }
        public string id { get; set; }
        public string s { get; set; }
        public int y { get; set; }
        public string q { get; set; }
        public object[] i { get; set; }
    }
}
