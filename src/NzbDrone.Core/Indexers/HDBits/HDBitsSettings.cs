using System;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System.Linq.Expressions;
using FluentValidation.Results;
using System.Collections.Generic;

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

            Categories = new int[] { (int)HdBitsCategory.Movie };
            Codecs = new int[0];
            Mediums = new int[0];
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

        [FieldDefinition(5, Label = "Categories", Type = FieldType.Tag, SelectOptions = typeof(HdBitsCategory), Advanced = true, HelpText = "Options: Movie, TV, Documentary, Music, Sport, Audio, XXX, MiscDemo. If unspecified, all options are used.")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(6, Label = "Codecs", Type = FieldType.Tag, SelectOptions = typeof(HdBitsCodec), Advanced = true, HelpText = "Options: h264, Mpeg2, VC1, Xvid. If unspecified, all options are used.")]
        public IEnumerable<int> Codecs { get; set; }

        [FieldDefinition(7, Label = "Mediums", Type = FieldType.Tag, SelectOptions = typeof(HdBitsMedium), Advanced = true, HelpText = "Options: BluRay, Encode, Capture, Remux, WebDL. If unspecified, all options are used.")]
        public IEnumerable<int> Mediums { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
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
