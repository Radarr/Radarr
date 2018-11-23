using System;
using System.Collections.Generic;
using Radarr.Http.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Radarr.Http;

namespace Radarr.Api.V2.Qualities
{
    public class QualityDefinitionModule : RadarrRestModule<QualityDefinitionResource>
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

            CreateResource = Create;
        }

        private int Create(QualityDefinitionResource qualityDefinitionResource)
        {
            throw new BadRequestException("Not allowed!");
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
    }
}
