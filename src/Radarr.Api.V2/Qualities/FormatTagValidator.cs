using System.Collections.Generic;
using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Core.CustomFormats;

namespace Radarr.Api.V2.Qualities
{
    public class FormatTagValidator : PropertyValidator
    {
        public FormatTagValidator() : base("{ValidationMessage}")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                context.SetMessage("Format Tags cannot be null!");
                return false;
            }

            var tags = (IEnumerable<string>) context.PropertyValue;

            var invalidTags = tags.Where(t => !FormatTag.QualityTagRegex.IsMatch(t));

            if (invalidTags.Count() == 0) return true;

            var formatMessage =
                $"Format Tags ({string.Join(", ", invalidTags)}) are in an invalid format! Check the Wiki to learn how they should look.";
            context.SetMessage(formatMessage);
            return false;
        }
    }

    public static class PropertyValidatorExtensions
    {
        public static void SetMessage(this PropertyValidatorContext context, string message, string argument = "ValidationMessage")
        {
            context.MessageFormatter.AppendArgument(argument, message);
        }
    }
}
