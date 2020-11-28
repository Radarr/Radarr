using FluentValidation.Validators;

namespace Radarr.Http.Validation
{
    public class ImportListSyncIntervalValidator : PropertyValidator
    {
        public ImportListSyncIntervalValidator()
            : base("Must be greater than 6 hours")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var value = (int)context.PropertyValue;

            if (value >= 6)
            {
                return true;
            }

            return false;
        }
    }
}
