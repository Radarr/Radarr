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
            : base("Title slug is in use by another artist with a similar name")
        {
            _artistService = artistService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            return !_artistService.GetAllArtists().Where(s => s.NameSlug.IsNotNullOrWhiteSpace())
                                                 .ToList()
                                                 .Exists(s => s.NameSlug.Equals(context.PropertyValue.ToString()) && s.Id != instanceId);
        }
    }
}
