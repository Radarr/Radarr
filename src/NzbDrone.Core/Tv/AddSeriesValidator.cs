using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Tv
{
    public interface IAddSeriesValidator
    {
        ValidationResult Validate(Series instance);
    }

    public class AddSeriesValidator : AbstractValidator<Series>, IAddSeriesValidator
    {
        public AddSeriesValidator(RootFolderValidator rootFolderValidator,
                                  SeriesTitleSlugValidator seriesTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator);

            RuleFor(c => c.TitleSlug).SetValidator(seriesTitleSlugValidator);
        }
    }
}
