using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats
{
    [TestFixture]
    public class CustomFormatsFixture : CoreTest
    {
        private static List<CustomFormat> _customFormats { get; set; }

        public static void GivenCustomFormats(params CustomFormat[] formats)
        {
            _customFormats = formats.ToList();
        }

        public static List<ProfileFormatItem> GetSampleFormatItems(params string[] allowed)
        {
            return _customFormats.Select(f => new ProfileFormatItem { Format = f, Allowed = allowed.Contains(f.Name) }).ToList();
        }

        public static List<ProfileFormatItem> GetDefaultFormatItems()
        {
            return new List<ProfileFormatItem>
            {
                new ProfileFormatItem
                {
                    Allowed = true,
                    Format = CustomFormat.None
                }
            };
        }
    }
}
