using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieValidator
    {
        ValidationResult Validate(Movie instance);
    }

    public class AddMovieValidator : AbstractValidator<Movie>, IAddMovieValidator
    {
        public AddMovieValidator(RootFolderValidator rootFolderValidator,
                                  MoviePathValidator moviePathValidator,
                                  MovieAncestorValidator movieAncestorValidator,
                                  MovieTitleSlugValidator movieTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(moviePathValidator)
                                .SetValidator(movieAncestorValidator);

            RuleFor(c => c.TitleSlug).SetValidator(movieTitleSlugValidator);
        }
    }
}
