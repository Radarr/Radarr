using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V3.Movies
{
    public class MovieEditorValidator : AbstractValidator<Movie>
    {
        public MovieEditorValidator(RootFolderExistsValidator rootFolderExistsValidator, QualityProfileExistsValidator qualityProfileExistsValidator)
        {
            RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => s.RootFolderPath.IsNotNullOrWhiteSpace());

            RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }
    }
}
