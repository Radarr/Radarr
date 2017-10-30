using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class ArtistSlugValidator : PropertyValidator
    {
        private readonly IArtistService _artistService;

        public ArtistSlugValidator(IArtistService artistService)
            : base("Name slug '{slug}' is in use by artist '{artistName}'")
        {
            _artistService = artistService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;
            var slug = context.PropertyValue.ToString();

            var conflictingArtist = _artistService.GetAllArtists()
                                                               .FirstOrDefault(s => s.NameSlug.IsNotNullOrWhiteSpace() &&
             s.NameSlug.Equals(context.PropertyValue.ToString()) &&
             s.Id != instanceId);

            if (conflictingArtist == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("slug", slug);
            context.MessageFormatter.AppendArgument("artistName", conflictingArtist.Name);

            return false;
        }
    }
}
