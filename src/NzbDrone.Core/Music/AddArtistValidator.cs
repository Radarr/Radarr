using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistValidator
    {
        ValidationResult Validate(Artist instance);
    }

    public class AddArtistValidator : AbstractValidator<Artist>, IAddArtistValidator
    {
        public AddArtistValidator(RootFolderValidator rootFolderValidator,
                                  ArtistPathValidator artistPathValidator,
                                  ArtistAncestorValidator artistAncestorValidator,
                                  ProfileExistsValidator profileExistsValidator,
                                  MetadataProfileExistsValidator metadataProfileExistsValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(artistPathValidator)
                                .SetValidator(artistAncestorValidator);

            RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);

            RuleFor(c => c.MetadataProfileId).SetValidator(metadataProfileExistsValidator);

        }
    }
}
