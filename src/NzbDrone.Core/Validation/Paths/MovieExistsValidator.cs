using System;
using FluentValidation.Validators;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class MovieExistsValidator : PropertyValidator
    {
        private readonly IMovieService _seriesService;

        public MovieExistsValidator(IMovieService seriesService)
            : base("This movie has already been added")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            int tmdbId = (int)context.PropertyValue;

            return (!_seriesService.GetAllMovies().Exists(s => s.TmdbId == tmdbId));
        }
    }
}