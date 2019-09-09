using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Lidarr.Api.V1.Profiles.Quality
{
    public static class QualityCutoffValidator
    {
        public static IRuleBuilderOptions<T, int> ValidCutoff<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new ValidCutoffValidator<T>());
        }
    }

    public class ValidCutoffValidator<T> : PropertyValidator
    {
        public ValidCutoffValidator()
            : base("Cutoff must be an allowed quality or group")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            int cutoff = (int)context.PropertyValue;
            dynamic instance = context.ParentContext.InstanceToValidate;
            var items = instance.Items as IList<QualityProfileQualityItemResource>;

            QualityProfileQualityItemResource cutoffItem = items.SingleOrDefault(i => (i.Quality == null && i.Id == cutoff) || i.Quality?.Id == cutoff);

            if (cutoffItem == null)
            {
                return false;
            }

            if (!cutoffItem.Allowed)
            {
                return false;
            }

            return true;
        }
    }
}
