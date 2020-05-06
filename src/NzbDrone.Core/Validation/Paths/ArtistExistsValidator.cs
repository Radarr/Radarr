using FluentValidation.Validators;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Validation.Paths
{
    public class ArtistExistsValidator : PropertyValidator
    {
        private readonly IArtistService _artistService;

        public ArtistExistsValidator(IArtistService artistService)
            : base("This artist has already been added.")
        {
            _artistService = artistService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_artistService.GetAllArtists().Exists(s => s.Metadata.Value.ForeignAuthorId == context.PropertyValue.ToString());
        }
    }
}
