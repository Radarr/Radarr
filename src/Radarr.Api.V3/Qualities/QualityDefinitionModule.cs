using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Qualities;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Qualities
{
    public class QualityDefinitionModule : RadarrRestModule<QualityDefinitionResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionModule(IQualityDefinitionService qualityDefinitionService)
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            Put("/update", d => UpdateMany());
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

        private object UpdateMany()
        {
            //Read from request
            var qualityDefinitions = Request.Body.FromJson<List<QualityDefinitionResource>>()
                                                 .ToModel()
                                                 .ToList();

            _qualityDefinitionService.UpdateMany(qualityDefinitions);

            return ResponseWithCode(_qualityDefinitionService.All()
                                    .ToResource(),
                                    HttpStatusCode.Accepted);
        }
    }
}
