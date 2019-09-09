using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Validation.Paths
{
    public class ArtistPathValidator : PropertyValidator
    {
        private readonly IArtistService _artistService;

        public ArtistPathValidator(IArtistService artistService)
            : base("Path is already configured for another artist")
        {
            _artistService = artistService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            return (!_artistService.GetAllArtists().Exists(s => s.Path.PathEquals(context.PropertyValue.ToString()) && s.Id != instanceId));
        }
    }
}
