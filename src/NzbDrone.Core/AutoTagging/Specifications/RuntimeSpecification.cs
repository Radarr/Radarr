using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class RuntimeSpecificationValidator : AbstractValidator<RuntimeSpecification>
    {
        public RuntimeSpecificationValidator()
        {
            RuleFor(c => c.Min).GreaterThanOrEqualTo(0);

            RuleFor(c => c.Max).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .GreaterThanOrEqualTo(c => c.Min);
        }
    }

    public class RuntimeSpecification : AutoTaggingSpecificationBase
    {
        private static readonly RuntimeSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Runtime";

        [FieldDefinition(1, Label = "Minimum Runtime", Type = FieldType.Number)]
        public int Min { get; set; }

        [FieldDefinition(2, Label = "Maximum Runtime", Type = FieldType.Number)]
        public int Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            return movie?.MovieMetadata?.Value?.Runtime != null &&
                   movie.MovieMetadata.Value.Runtime >= Min &&
                   movie.MovieMetadata.Value.Runtime <= Max;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
