using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class MoviePathValidator : PropertyValidator
    {
        private readonly IMovieService _seriesService;

        public MoviePathValidator(IMovieService seriesService)
            : base("Path is already configured for another series")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            return (!_seriesService.GetAllMovies().Exists(s => s.Path.PathEquals(context.PropertyValue.ToString()) && s.Id != instanceId));
        }
    }
}