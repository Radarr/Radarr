using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class StudioSpecificationValidator : AbstractValidator<StudioSpecification>
    {
        public StudioSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class StudioSpecification : AutoTaggingSpecificationBase
    {
        private static readonly StudioSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Studio";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationStudio", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            return Value.Any(studio => movie.MovieMetadata?.Value?.Studio?.EqualsIgnoreCase(studio) ?? false);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
