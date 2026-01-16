namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class TMDbTranslationsResponse
    {
        public int Id { get; set; }
        public TranslationResource[] Translations { get; set; }
    }
}
