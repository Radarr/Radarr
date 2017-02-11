using FluentValidation.Validators;

namespace NzbDrone.Api.Validation
{
    public class RssSyncIntervalValidator : PropertyValidator
    {
        public RssSyncIntervalValidator()
            : base("Must be between 10 and 720 or 0 to disable")
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

            if (value >= 10 && value <= 720)
            {
                return true;
            }

            return false;
        }
    }
}
