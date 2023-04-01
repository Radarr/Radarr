using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        public static IRuleBuilderOptions<T, string> ValidMovieFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MovieTitleRegex)).WithMessage("Must contain movie title");
        }

        public static IRuleBuilderOptions<T, string> ValidMovieFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MovieTitleRegex)).WithMessage("Must contain movie title");
        }
    }
}
