using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.Validation.Paths
{
    public class AuthorPathValidator : PropertyValidator
    {
        private readonly IAuthorService _authorService;

        public AuthorPathValidator(IAuthorService authorService)
            : base("Path is already configured for another author")
        {
            _authorService = authorService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            return !_authorService.AllAuthorPaths().Any(s => s.Value.PathEquals(context.PropertyValue.ToString()) && s.Key != instanceId);
        }
    }
}
