using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authors;

namespace NzbDrone.Core.Validation.Paths
{
    public class AuthorPathValidator : PropertyValidator
    {
        private readonly IAuthorService _authorService;

        public AuthorPathValidator(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for an existing author";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;
            var path = context.PropertyValue.ToString();

            context.MessageFormatter.AppendArgument("path", path);

            var existingAuthors = _authorService.GetAllAuthors();

            return !existingAuthors.Any(a => a.Id != instanceId &&
                                             a.Path.IsPathValid(PathValidationType.CurrentOs) &&
                                             a.Path.PathEquals(path));
        }
    }
}
