using System;
using FluentValidation.Validators;

namespace NzbDrone.Core.Validation
{
    public class GuidValidator : PropertyValidator
    {
        public GuidValidator()
            : base("String is not a valid Guid")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;
            
            return Guid.TryParse(context.PropertyValue.ToString(), out Guid guidOutput);
        }
    }
}
