using NzbDrone.Core.Localization;
using Radarr.Http;

namespace Radarr.Api.V3.Localization
{
    public class LocalizationModule : RadarrRestModule<LocalizationResource>
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationModule(ILocalizationService localizationService)
        {
            _localizationService = localizationService;

            GetResourceSingle = GetLocalizationDictionary;
        }

        private LocalizationResource GetLocalizationDictionary()
        {
            return _localizationService.GetLocalizationDictionary().ToResource();
        }
    }
}
