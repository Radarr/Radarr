using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistValidator
    {
        ValidationResult Validate(Author instance);
    }

    public class AddArtistValidator : AbstractValidator<Author>, IAddArtistValidator
    {
        public AddArtistValidator(RootFolderValidator rootFolderValidator,
                                  ArtistPathValidator artistPathValidator,
                                  ArtistAncestorValidator artistAncestorValidator,
                                  QualityProfileExistsValidator qualityProfileExistsValidator,
                                  MetadataProfileExistsValidator metadataProfileExistsValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(artistPathValidator)
                                .SetValidator(artistAncestorValidator);

            RuleFor(c => c.QualityProfileId).SetValidator(qualityProfileExistsValidator);

            RuleFor(c => c.MetadataProfileId).SetValidator(metadataProfileExistsValidator);
        }
    }
}
