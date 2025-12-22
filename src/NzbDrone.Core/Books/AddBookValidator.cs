using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Books
{
    public interface IAddBookValidator
    {
        ValidationResult Validate(Book instance);
    }

    public class AddBookValidator : AbstractValidator<Book>, IAddBookValidator
    {
        public AddBookValidator(RootFolderValidator rootFolderValidator,
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
