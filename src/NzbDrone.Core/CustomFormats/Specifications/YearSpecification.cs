using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class YearSpecificationValidator : AbstractValidator<YearSpecification>
    {
        public YearSpecificationValidator()
        {
            RuleFor(c => c.Min).NotEmpty().GreaterThan(0);
            RuleFor(c => c.Max).NotEmpty().GreaterThanOrEqualTo(c => c.Min);
        }
    }

    public class YearSpecification : CustomFormatSpecificationBase
    {
        private static readonly YearSpecificationValidator Validator = new ();

        public override int Order => 10;
        public override string ImplementationName => "Year";

        [FieldDefinition(1, Label = "Minimum Year", Type = FieldType.Number)]
        public int Min { get; set; }

        [FieldDefinition(2, Label = "Maximum Year", Type = FieldType.Number)]
        public int Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var year = input.MovieInfo?.Year ?? input.Movie?.MovieMetadata?.Value?.Year;

            return year >= Min && year <= Max;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
