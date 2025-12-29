using FluentValidation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistValidator : IValidator<Artist>
    {
    }

    public class AddArtistValidator : AbstractValidator<Artist>, IAddArtistValidator
    {
        public AddArtistValidator(RootFolderValidator rootFolderValidator,
                                  ArtistPathValidator artistPathValidator,
                                  SystemFolderValidator systemFolderValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(artistPathValidator)
                .SetValidator(systemFolderValidator);

            RuleFor(c => c.Name).NotEmpty();
        }
    }
}
