using FluentValidation.Validators;

namespace NzbDrone.Core.Music
{
    public class AlbumPathValidator : PropertyValidator
    {
        private readonly IAlbumService _albumService;

        public AlbumPathValidator(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        protected override string GetDefaultMessageTemplate() => "Path is already configured for another album";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var path = context.PropertyValue.ToString();

            return !_albumService.AlbumPathExists(path);
        }
    }
}
