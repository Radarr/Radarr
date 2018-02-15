using System.Collections.Generic;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Qualities
{
    public class QualityDefinitionModule : NzbDroneRestModule<QualityDefinitionResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IParsingService _parsingService;

        public QualityDefinitionModule(IQualityDefinitionService qualityDefinitionService, IParsingService parsingService)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _parsingService = parsingService;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            Get["/test"] = x => Test();
        }

        private void Update(QualityDefinitionResource resource)
        {
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
        }

        private QualityDefinitionResource GetById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        private List<QualityDefinitionResource> GetAll()
        {
            return _qualityDefinitionService.All().ToResource();
        }

        private QualityDefinitionTestResource Test()
        {

            var parsed = _parsingService.ParseMovieInfo((string) Request.Query.title);
            if (parsed == null)
            {
                return null;
            }
            return new QualityDefinitionTestResource
            {
                Matches = _parsingService.MatchQualityTags(parsed).ToResource(),
                BestMatch = parsed.Quality.QualityDefinition.ToResource()
            };
        }
    }
}