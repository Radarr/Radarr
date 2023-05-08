using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class QualityModifierSpecificationValidator : AbstractValidator<QualityModifierSpecification>
    {
        public QualityModifierSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
            RuleFor(c => c.Value).Custom((qualityValue, context) =>
            {
                if (!Enum.IsDefined(typeof(Modifier), qualityValue))
                {
                    context.AddFailure(string.Format("Invalid quality modifier condition value: {0}", qualityValue));
                }
            });
        }
    }

    public class QualityModifierSpecification : CustomFormatSpecificationBase
    {
        private static readonly QualityModifierSpecificationValidator Validator = new QualityModifierSpecificationValidator();

        public override int Order => 7;
        public override string ImplementationName => "Quality Modifier";

        [FieldDefinition(1, Label = "Quality Modifier", Type = FieldType.Select, SelectOptions = typeof(Modifier))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.MovieInfo?.Quality?.Quality?.Modifier ?? (int)Modifier.NONE) == (Modifier)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
