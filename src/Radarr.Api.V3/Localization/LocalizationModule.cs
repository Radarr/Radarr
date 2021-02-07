using System.Text.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Localization;
using Radarr.Http;

namespace Radarr.Api.V3.Localization
{
    public class LocalizationModule : RadarrRestModule<LocalizationResource>
    {
        private readonly ILocalizationService _localizationService;
        private readonly JsonSerializerOptions _serializerSettings;

        public LocalizationModule(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _serializerSettings = STJson.GetSerializerSettings();
            _serializerSettings.DictionaryKeyPolicy = null;
            _serializerSettings.PropertyNamingPolicy = null;

            Get("/", x => GetLocalizationDictionary());
        }

        private string GetLocalizationDictionary()
        {
            return JsonSerializer.Serialize(_localizationService.GetLocalizationDictionary().ToResource(), _serializerSettings);
        }
    }
}
