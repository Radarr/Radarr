using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class GenreSpecificationValidator : AbstractValidator<GenreSpecification>
    {
        public GenreSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class GenreSpecification : AutoTaggingSpecificationBase
    {
        private static readonly GenreSpecificationValidator Validator = new ();

        public override int Order => 2;
        public override string ImplementationName => "Genre";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationGenre", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Movie movie)
        {
            return movie?.MovieMetadata?.Value?.Genres.Any(genre => Value.ContainsIgnoreCase(genre)) ?? false;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
