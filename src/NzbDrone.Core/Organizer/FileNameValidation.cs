using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        internal static readonly Regex OriginalTokenRegex = new (@"(\{Original[- ._](?:Title|Filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidMovieFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidMovieFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidMovieFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidMovieFolderFormatValidator());
        }
    }

    public class ValidMovieFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain movie title and release year OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return (FileNameBuilder.MovieTitleRegex.IsMatch(value) && FileNameBuilder.ReleaseYearRegex.IsMatch(value)) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
        }
    }

    public class ValidMovieFolderFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain movie title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            // TODO: Deprecate OriginalTokenRegex use for Movie Folder Format
            return FileNameBuilder.MovieTitleRegex.IsMatch(value) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
        }
    }

    public class IllegalCharactersValidator : PropertyValidator
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        protected override string GetDefaultMessageTemplate() => "Contains illegal characters: {InvalidCharacters}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;
            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var invalidCharacters = InvalidPathChars.Where(i => value!.IndexOf(i) >= 0).ToList();
            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }
}
