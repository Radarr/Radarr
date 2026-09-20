using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class CustomFormatSpecificationValidator : AbstractValidator<CustomFormatSpecification>
    {
        public CustomFormatSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class CustomFormatSpecification : AutoTaggingSpecificationBase
    {
        private static readonly CustomFormatSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Custom Format";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationCustomFormat", Type = FieldType.CustomFormat)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            if (movie.MatchedCustomFormatIds == null)
            {
                return false;
            }

            return movie.MatchedCustomFormatIds.Contains(Value);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
