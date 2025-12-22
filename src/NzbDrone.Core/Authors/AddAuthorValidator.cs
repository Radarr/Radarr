using FluentValidation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Authors
{
    public interface IAddAuthorValidator : IValidator<Author>
    {
    }

    public class AddAuthorValidator : AbstractValidator<Author>, IAddAuthorValidator
    {
        public AddAuthorValidator(RootFolderValidator rootFolderValidator,
                                  AuthorPathValidator authorPathValidator,
                                  SystemFolderValidator systemFolderValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(authorPathValidator)
                .SetValidator(systemFolderValidator);

            RuleFor(c => c.Name).NotEmpty();
        }
    }
}
