using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class StatusSpecificationValidator : AbstractValidator<StatusSpecification>
    {
        public StatusSpecificationValidator()
        {
            RuleFor(c => c.Status).Custom((statusType, context) =>
            {
                if (!Enum.IsDefined(typeof(MovieStatusType), statusType))
                {
                    context.AddFailure($"Invalid status type condition value: {statusType}");
                }
            });
        }
    }

    public class StatusSpecification : AutoTaggingSpecificationBase
    {
        private static readonly StatusSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Status";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationStatus", Type = FieldType.Select, SelectOptions = typeof(MovieStatusType))]
        public int Status { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            return movie?.MovieMetadata?.Value?.Status == (MovieStatusType)Status;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
