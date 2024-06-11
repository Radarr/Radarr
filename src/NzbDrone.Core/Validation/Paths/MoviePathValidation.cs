using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Validation.Paths
{
    public class MoviePathValidator : PropertyValidator
    {
        private readonly IMovieService _moviesService;

        public MoviePathValidator(IMovieService moviesService)
        {
            _moviesService = moviesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for an existing movie";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            // Skip the path for this movie and any invalid paths
            return !_moviesService.AllMoviePaths().Any(s => s.Key != instanceId &&
                                                            s.Value.IsPathValid(PathValidationType.CurrentOs) &&
                                                            s.Value.PathEquals(context.PropertyValue.ToString()));
        }
    }
}
