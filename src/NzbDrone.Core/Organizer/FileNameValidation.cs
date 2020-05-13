using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        internal static readonly Regex OriginalTokenRegex = new Regex(@"(\{original[- ._](?:title|filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidBookFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new ValidStandardTrackFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidAuthorFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.AuthorNameRegex)).WithMessage("Must contain Author name");
        }
    }

    public class ValidStandardTrackFormatValidator : PropertyValidator
    {
        public ValidStandardTrackFormatValidator()
            : base("Must contain Book Title")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (!FileNameBuilder.BookTitleRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }
}
