using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class MovieAncestorValidator : PropertyValidator
    {
        private readonly IMovieService _seriesService;

        public MovieAncestorValidator(IMovieService seriesService)
            : base("Path is an ancestor of an existing path")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            return !_seriesService.GetAllMovies().Any(s => context.PropertyValue.ToString().IsParentPath(s.Path));
        }
    }
}