using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Movies
{
    public class MovieTitleSlugValidator : PropertyValidator
    {
        private readonly IMovieService _movieService;

        public MovieTitleSlugValidator(IMovieService movieService)
            : base("Title slug '{slug}' is in use by movie '{movieTitle}'")
        {
            _movieService = movieService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;
            var slug = context.PropertyValue.ToString();

            var conflictingMovie = _movieService.GetAllMovies()
                                                  .FirstOrDefault(s => s.TitleSlug.IsNotNullOrWhiteSpace() &&
                                                              s.TitleSlug.Equals(context.PropertyValue.ToString()) &&
                                                              s.Id != instanceId);

            if (conflictingMovie == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("slug", slug);
            context.MessageFormatter.AppendArgument("movieTitle", conflictingMovie.Title);

            return false;
        }
    }
}
