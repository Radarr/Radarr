using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface IAddArtistValidator
    {
        ValidationResult Validate(Artist instance);
    }

    public class AddArtistValidator : AbstractValidator<Artist>, IAddArtistValidator
    {
        public AddArtistValidator(RootFolderValidator rootFolderValidator,
                                  SeriesPathValidator seriesPathValidator,
                                  DroneFactoryValidator droneFactoryValidator,
                                  SeriesAncestorValidator seriesAncestorValidator,
                                  ArtistSlugValidator artistTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(seriesPathValidator)
                                .SetValidator(droneFactoryValidator)
                                .SetValidator(seriesAncestorValidator);

            RuleFor(c => c.NameSlug).SetValidator(artistTitleSlugValidator);// TODO: Check if we are going to use a slug or artistName
        }
    }
}
