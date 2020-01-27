using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Restrictions;

namespace Radarr.Http.RESTrictions
{
    public class RestrictionModule : RadarrRestModule<RestrictionResource>
    {
        private readonly IRestrictionService _restrictionService;

        public RestrictionModule(IRestrictionService restrictionService)
        {
            _restrictionService = restrictionService;

            GetResourceById = GetRestriction;
            GetResourceAll = GetAllRestrictions;
            CreateResource = CreateRestriction;
            UpdateResource = UpdateRestriction;
            DeleteResource = DeleteRestriction;

            SharedValidator.RuleFor(r => r).Custom((restriction, context) =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace())
                {
                    context.AddFailure("Either 'Must contain' or 'Must not contain' is required");
                }
            });
        }

        private RestrictionResource GetRestriction(int id)
        {
            return _restrictionService.Get(id).ToResource();
        }

        private List<RestrictionResource> GetAllRestrictions()
        {
            return _restrictionService.All().ToResource();
        }

        private int CreateRestriction(RestrictionResource resource)
        {
            return _restrictionService.Add(resource.ToModel()).Id;
        }

        private void UpdateRestriction(RestrictionResource resource)
        {
            _restrictionService.Update(resource.ToModel());
        }

        private void DeleteRestriction(int id)
        {
            _restrictionService.Delete(id);
        }
    }
}
