using FluentValidation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Music
{
    public interface IAddAlbumValidator : IValidator<Album>
    {
    }

    public class AddAlbumValidator : AbstractValidator<Album>, IAddAlbumValidator
    {
        public AddAlbumValidator(RootFolderValidator rootFolderValidator,
                                 AlbumPathValidator albumPathValidator,
                                 SystemFolderValidator systemFolderValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(albumPathValidator)
                .SetValidator(systemFolderValidator)
                .When(c => !string.IsNullOrWhiteSpace(c.Path));

            RuleFor(c => c.Title).NotEmpty();
        }
    }
}
