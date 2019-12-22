using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Restrictions;
using Radarr.Http;

namespace Radarr.Api.V3.Restrictions
{
    public class RestrictionModule : RadarrRestModule<RestrictionResource>
    {
        private readonly IRestrictionService _restrictionService;

        public RestrictionModule(IRestrictionService restrictionService)
        {
            _restrictionService = restrictionService;

            GetResourceById = GetById;
            GetResourceAll = GetAll;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = DeleteRestriction;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace())
                {
                    context.AddFailure("Either 'Must contain' or 'Must not contain' is required");
                }
            });
        }

        private RestrictionResource GetById(int id)
        {
            return _restrictionService.Get(id).ToResource();
        }

        private List<RestrictionResource> GetAll()
        {
            return _restrictionService.All().ToResource();
        }

        private int Create(RestrictionResource resource)
        {
            return _restrictionService.Add(resource.ToModel()).Id;
        }

        private void Update(RestrictionResource resource)
        {
            _restrictionService.Update(resource.ToModel());
        }

        private void DeleteRestriction(int id)
        {
            _restrictionService.Delete(id);
        }
    }
}
