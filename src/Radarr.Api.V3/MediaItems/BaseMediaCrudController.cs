using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.MediaItems
{
    public abstract class BaseMediaCrudController<TResource, TModel> : RestControllerWithSignalR<TResource, TModel>
        where TResource : RestResource, new()
        where TModel : ModelBase, new()
    {
        protected abstract IBaseMediaService<TModel> MediaService { get; }
        protected abstract IRootFolderService RootFolderService { get; }

        protected BaseMediaCrudController(IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
        }

        protected void SetupPathValidation(
            RootFolderValidator rootFolderValidator,
            MappedNetworkDriveValidator mappedNetworkDriveValidator,
            RecycleBinValidator recycleBinValidator,
            SystemFolderValidator systemFolderValidator,
            RootFolderExistsValidator rootFolderExistsValidator)
        {
            SharedValidator.RuleFor(s => GetPath(s)).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => GetPath(s).IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => GetPath(s)).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => GetRootFolderPath(s).IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => GetRootFolderPath(s)).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => GetPath(s).IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => GetPath(s)).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();
        }

        protected void SetupQualityValidation(QualityProfileExistsValidator qualityProfileExistsValidator)
        {
            SharedValidator.RuleFor(s => GetQualityProfileId(s)).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }

        protected void SetupTitleValidation()
        {
            PostValidator.RuleFor(s => GetTitle(s)).NotEmpty();
        }

        protected abstract string GetPath(TResource resource);
        protected abstract string GetRootFolderPath(TResource resource);
        protected abstract int GetQualityProfileId(TResource resource);
        protected abstract string GetTitle(TResource resource);

        protected abstract TResource MapToResource(TModel model);
        protected abstract TModel ResourceToModel(TResource resource);
        protected abstract TModel ApplyResourceToModel(TResource resource, TModel model);

        protected override TResource GetResourceById(int id)
        {
            var model = MediaService.Get(id);
            return MapToResource(model);
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual ActionResult<TResource> CreateResource([FromBody] TResource resource)
        {
            var model = ResourceToModel(resource);
            var added = MediaService.Add(model);
            return Created(added.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual ActionResult<TResource> UpdateResource([FromBody] TResource resource)
        {
            var existingModel = MediaService.Get(resource.Id);
            var updatedModel = ApplyResourceToModel(resource, existingModel);
            var result = MediaService.Update(updatedModel);
            var resultResource = MapToResource(result);

            BroadcastResourceChange(ModelAction.Updated, resultResource);

            return Ok(resultResource);
        }

        [RestDeleteById]
        public virtual ActionResult DeleteResource(int id, bool deleteFiles = false)
        {
            MediaService.Delete(id, deleteFiles);
            return NoContent();
        }
    }
}
