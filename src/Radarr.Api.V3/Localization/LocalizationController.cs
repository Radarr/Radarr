using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Localization;
using Radarr.Http;

namespace Radarr.Api.V3.Localization
{
    [V3ApiController]
    public class LocalizationController : Controller
    {
        private readonly ILocalizationService _localizationService;
        private readonly JsonSerializerOptions _serializerSettings;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _serializerSettings = STJson.GetSerializerSettings();
            _serializerSettings.DictionaryKeyPolicy = null;
            _serializerSettings.PropertyNamingPolicy = null;
        }

        [HttpGet]
        [Produces("application/json")]
        public JsonHttpResult<LocalizationResource> GetLocalizationDictionary()
        {
            return TypedResults.Json(_localizationService.GetLocalizationDictionary().ToResource(), _serializerSettings);
        }

        [HttpGet("language")]
        [Produces("application/json")]
        public Ok<LocalizationLanguageResource> GetLanguage()
        {
            var identifier = _localizationService.GetLanguageIdentifier();

            return TypedResults.Ok(new LocalizationLanguageResource
            {
                Identifier = identifier
            });
        }
    }
}
