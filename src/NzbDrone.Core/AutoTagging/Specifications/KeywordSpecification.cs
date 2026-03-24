using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class KeywordSpecificationValidator : AbstractValidator<KeywordSpecification>
    {
        public KeywordSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class KeywordSpecification : AutoTaggingSpecificationBase
    {
        private static readonly KeywordSpecificationValidator Validator = new ();

        public override int Order => 2;
        public override string ImplementationName => "Keyword";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationKeyword", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            return movie?.MovieMetadata?.Value?.Keywords.Any(keyword => Value.ContainsIgnoreCase(keyword)) ?? false;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
