using FluentValidation.Validators;

namespace Radarr.Http.Validation
{
    public class ImportListSyncIntervalValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must be greater than 6 hours";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var value = (int)context.PropertyValue;

            return value >= 6;
        }
    }
}
