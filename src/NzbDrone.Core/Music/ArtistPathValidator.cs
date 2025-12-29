using FluentValidation.Validators;

namespace NzbDrone.Core.Music
{
    public class ArtistPathValidator : PropertyValidator
    {
        private readonly IArtistService _artistService;

        public ArtistPathValidator(IArtistService artistService)
        {
            _artistService = artistService;
        }

        protected override string GetDefaultMessageTemplate() => "Path is already configured for another artist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var path = context.PropertyValue.ToString();

            return !_artistService.ArtistPathExists(path);
        }
    }
}
