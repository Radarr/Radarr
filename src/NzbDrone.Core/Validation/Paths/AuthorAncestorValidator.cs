using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.Validation.Paths
{
    public class AuthorAncestorValidator : PropertyValidator
    {
        private readonly IAuthorService _authorService;

        public AuthorAncestorValidator(IAuthorService authorService)
            : base("Path is an ancestor of an existing author")
        {
            _authorService = authorService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_authorService.AllAuthorPaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
        }
    }
}
