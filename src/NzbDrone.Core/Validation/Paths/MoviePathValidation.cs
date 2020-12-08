using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Validation.Paths
{
    public class MoviePathValidator : PropertyValidator
    {
        private readonly IMovieService _moviesService;

        public MoviePathValidator(IMovieService moviesService)
            : base("Path is already configured for an existing movie: {moviePath}")
        {
            _moviesService = moviesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            context.MessageFormatter.AppendArgument("moviePath", context.PropertyValue.ToString());

            return !_moviesService.AllMoviePaths().Any(s => s.Value.PathEquals(context.PropertyValue.ToString()) && s.Key != instanceId);
        }
    }
}
