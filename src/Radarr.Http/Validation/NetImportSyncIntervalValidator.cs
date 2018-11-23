using FluentValidation.Validators;

namespace Radarr.Http.Validation
{
    public class NetImportSyncIntervalValidator : PropertyValidator
    {
        public NetImportSyncIntervalValidator()
            : base("Must be between 10 and 1440 or 0 to disable")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var value = (int)context.PropertyValue;

            if (value == 0)
            {
                return true;
            }

            if (value >= 10 && value <= 1440)
            {
                return true;
            }

            return false;
        }
    }
}
