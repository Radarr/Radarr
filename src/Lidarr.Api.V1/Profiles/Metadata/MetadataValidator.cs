using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Lidarr.Api.V1.Profiles.Metadata
{
    public static class MetadataValidation
    {
        public static IRuleBuilderOptions<T, IList<ProfilePrimaryAlbumTypeItemResource>> MustHaveAllowedPrimaryType<T>(this IRuleBuilder<T, IList<ProfilePrimaryAlbumTypeItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new PrimaryTypeValidator<T>());
        }

        public static IRuleBuilderOptions<T, IList<ProfileSecondaryAlbumTypeItemResource>> MustHaveAllowedSecondaryType<T>(this IRuleBuilder<T, IList<ProfileSecondaryAlbumTypeItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new SecondaryTypeValidator<T>());
        }

        public static IRuleBuilderOptions<T, IList<ProfileReleaseStatusItemResource>> MustHaveAllowedReleaseStatus<T>(this IRuleBuilder<T, IList<ProfileReleaseStatusItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new ReleaseStatusValidator<T>());
        }
    }


    public class PrimaryTypeValidator<T> : PropertyValidator
    {
        public PrimaryTypeValidator()
            : base("Must have at least one allowed primary type")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<ProfilePrimaryAlbumTypeItemResource>;

            if (list == null)
            {
                return false;
            }

            if (!list.Any(c => c.Allowed))
            {
                return false;
            }

            return true;
        }
    }

    public class SecondaryTypeValidator<T> : PropertyValidator
    {
        public SecondaryTypeValidator()
            : base("Must have at least one allowed secondary type")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<ProfileSecondaryAlbumTypeItemResource>;

            if (list == null)
            {
                return false;
            }

            if (!list.Any(c => c.Allowed))
            {
                return false;
            }

            return true;
        }
    }

    public class ReleaseStatusValidator<T> : PropertyValidator
    {
        public ReleaseStatusValidator()
            : base("Must have at least one allowed release status")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<ProfileReleaseStatusItemResource>;

            if (list == null)
            {
                return false;
            }

            if (!list.Any(c => c.Allowed))
            {
                return false;
            }

            return true;
        }
    }
}
