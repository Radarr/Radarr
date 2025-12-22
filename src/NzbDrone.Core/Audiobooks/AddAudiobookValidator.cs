using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Audiobooks
{
    public interface IAddAudiobookValidator
    {
        ValidationResult Validate(Audiobook instance);
    }

    public class AddAudiobookValidator : AbstractValidator<Audiobook>, IAddAudiobookValidator
    {
        public AddAudiobookValidator(RootFolderValidator rootFolderValidator,
                                     RecycleBinValidator recycleBinValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(recycleBinValidator);

            RuleFor(c => c.Title).NotEmpty();
        }
    }
}
