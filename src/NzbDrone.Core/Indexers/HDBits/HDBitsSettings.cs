using System;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System.Linq.Expressions;
using FluentValidation.Results;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsSettingsValidator : AbstractValidator<HDBitsSettings>
    {
        public HDBitsSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class HDBitsSettings : IProviderConfig
    {
        private static readonly HDBitsSettingsValidator Validator = new HDBitsSettingsValidator();

        public HDBitsSettings()
        {
            BaseUrl = "https://hdbits.org";
        }

        [FieldDefinition(0, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(3, Label = "Prefer Internal", Type = FieldType.Checkbox, HelpText = "Favors Internal releases over all other releases.")]
        public bool PreferInternal { get; set; }

        [FieldDefinition(4, Label = "Require Internal", Type = FieldType.Checkbox, HelpText = "Require Internal releases for release to be accepted.")]
        public bool RequireInternal { get; set; }

        [FieldDefinition(5, Label = "Categories", Advanced = true, HelpText = "A comma delimited list of integers. Options: 1=Movie, 2=TV, 3=Documentary, 4=Music, 5=Sport, 6=Audio, 7=XXX, 8=Misc/Demo. Example: 1,3")]
        public string Categories { get; set; }

        [FieldDefinition(6, Label = "Codec", Advanced = true, HelpText = "A comma delimited list of integers. Options: 1=h264, 2=Mpeg2, 3=VC1, 4=Xvid. Example: 1,2")]
        public string Codec { get; set; }

        [FieldDefinition(7, Label = "Medium", Advanced = true, HelpText = "A comma delimited list of integers. Options: 1=BluRay, 3=Encode, 4=Capture, 5=Remux, 6=WebDL. Example: 3,4,6")]
        public string Medium { get; set; }

        public int[] GetIntList(string input)
        {
            if (!String.IsNullOrWhiteSpace(input))
                return input.Split(',').Select(t => int.Parse(t.Trim())).ToArray();

            return new int[0];
        }

        public NzbDroneValidationResult Validate()
        {
            var results = Validator.Validate(this);

            Validate<string, HdBitsCategory>(results, () => this.Categories);
            Validate<string, HdBitsCodec>(results, () => this.Codec);
            Validate<string, HdBitsMedium>(results, () => this.Medium);

            return new NzbDroneValidationResult(results);
        }

        private void Validate<T, K>(ValidationResult results, Expression<Func<T>> expr) where K: struct
        {
            var propertyName = ((MemberExpression)expr.Body).Member.Name;
            var propertyValue = expr.Compile()() as string;

            // Nothing selected means no filtering, which is valid
            if (String.IsNullOrWhiteSpace(propertyValue))
                return;

            try
            {
                // I am not using ToIntList because that assumes the data is valid. This way doesn't
                // assume that and we try to identify specifically if a value is not an integer.

                var choices = Enum.GetValues(typeof(K)).Cast<K>().ToLookup(k => Convert.ToInt32(k));
                var candidates = propertyValue.Split(',').Select(t => t.Trim());

                foreach (var candidate in candidates)
                {
                    if (int.TryParse(candidate, out int v))
                    {
                        if (!choices.Contains(v))
                            results.Errors.Add(new ValidationFailure(propertyName, $"\"{candidate}\" is not a valid choice", candidate));
                    }
                    else
                    {
                        results.Errors.Add(new ValidationFailure(propertyName, $"\"{candidate}\" is not a valid number", candidate));
                    }
                }
            }
            catch
            {
                results.Errors.Add(new ValidationFailure(propertyName, "Uknown error validating property"));
            }
        }
    }

    public enum HdBitsCategory
    {
        Movie = 1,
        Tv = 2,
        Documentary = 3,
        Music = 4,
        Sport = 5,
        Audio = 6,
        Xxx = 7,
        MiscDemo = 8
    }

    public enum HdBitsCodec
    {
        H264 = 1,
        Mpeg2 = 2,
        Vc1 = 3,
        Xvid = 4
    }

    public enum HdBitsMedium
    {
        Bluray = 1,
        Encode = 3,
        Capture = 4,
        Remux = 5,
        WebDl = 6
    }
}
