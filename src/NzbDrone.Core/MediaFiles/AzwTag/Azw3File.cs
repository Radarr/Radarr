namespace NzbDrone.Core.MediaFiles.Azw
{
    public class Azw3File : AzwFile
    {
        public Azw3File(string path)
        : base(path)
        {
            MobiHeader = new MobiHeader(GetSectionData(0));
        }

        public string Title => MobiHeader.Title;
        public string Author => MobiHeader.ExtMeta.StringOrNull(100);
        public string Isbn => MobiHeader.ExtMeta.StringOrNull(104);
        public string Asin => MobiHeader.ExtMeta.StringOrNull(113);
        public string PublishDate => MobiHeader.ExtMeta.StringOrNull(106);
        public string Publisher => MobiHeader.ExtMeta.StringOrNull(101);
        public string Imprint => MobiHeader.ExtMeta.StringOrNull(102);
        public string Description => MobiHeader.ExtMeta.StringOrNull(103);
        public string Source => MobiHeader.ExtMeta.StringOrNull(112);
        public string Language => MobiHeader.ExtMeta.StringOrNull(524);
        public uint Version => MobiHeader.Version;
        public uint MobiType => MobiHeader.MobiType;

        private MobiHeader MobiHeader { get; set; }
    }
}
