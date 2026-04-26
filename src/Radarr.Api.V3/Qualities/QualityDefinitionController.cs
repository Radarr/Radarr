using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Qualities
{
    [V3ApiController]
    public class QualityDefinitionController :
        RestControllerWithSignalR<QualityDefinitionResource, QualityDefinition>,
        IHandle<CommandExecutedEvent>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionController(
            IQualityDefinitionService qualityDefinitionService,
            IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _qualityDefinitionService = qualityDefinitionService;

            SharedValidator.RuleFor(c => c)
                .SetValidator(new QualityDefinitionResourceValidator());
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Accepted<QualityDefinitionResource>, NotFound> Update([FromBody] QualityDefinitionResource resource)
        {
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
            return Accepted(model.Id);
        }

        protected override QualityDefinitionResource GetResourceById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<QualityDefinitionResource>> GetAll()
        {
            return TypedResults.Ok(_qualityDefinitionService.All().ToResource());
        }

        [HttpPut("update")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Ok<List<QualityDefinitionResource>> UpdateMany([FromBody] List<QualityDefinitionResource> resource)
        {
            // Read from request
            var qualityDefinitions = resource.ToModel().ToList();

            _qualityDefinitionService.UpdateMany(qualityDefinitions);

            return TypedResults.Ok(_qualityDefinitionService.All().ToResource());
        }

        [HttpGet("limits")]
        [Produces("application/json")]
        public Ok<QualityDefinitionLimitsResource> GetLimits()
        {
            return TypedResults.Ok(new QualityDefinitionLimitsResource(
                QualityDefinitionLimits.Min,
                QualityDefinitionLimits.Max));
        }

        [NonAction]
        public void Handle(CommandExecutedEvent message)
        {
            if (message.Command.Name == "ResetQualityDefinitions")
            {
                BroadcastResourceChange(ModelAction.Sync);
            }
        }
    }
}
