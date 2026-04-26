using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.SignalR;
using Radarr.Http;

namespace Radarr.Api.V3.Metadata
{
    [V3ApiController]
    public class MetadataController : ProviderControllerBase<MetadataResource, MetadataBulkResource, IMetadata, MetadataDefinition>
    {
        public static readonly MetadataResourceMapper ResourceMapper = new();
        public static readonly MetadataBulkResourceMapper BulkResourceMapper = new();

        public MetadataController(IBroadcastSignalRMessage signalRBroadcaster, IMetadataFactory metadataFactory)
            : base(signalRBroadcaster, metadataFactory, "metadata", ResourceMapper, BulkResourceMapper)
        {
        }

        [NonAction]
        public override Results<Ok<IEnumerable<MetadataResource>>, BadRequest> UpdateProvider([FromBody] MetadataBulkResource providerResource)
        {
            throw new NotImplementedException();
        }

        [NonAction]
        public override Ok<object> DeleteProviders([FromBody] MetadataBulkResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
