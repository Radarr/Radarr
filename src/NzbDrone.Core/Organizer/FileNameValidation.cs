using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        internal static readonly Regex OriginalTokenRegex = new Regex(@"(\{original[- ._](?:title|filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidTrackFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new ValidStandardTrackFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidArtistFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.ArtistNameRegex)).WithMessage("Must contain Artist name");
        }
    }

    public class ValidStandardTrackFormatValidator : PropertyValidator
    {
        public ValidStandardTrackFormatValidator()
            : base("Must contain Album Title")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (!FileNameBuilder.AlbumTitleRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }
}
