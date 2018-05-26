using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormat
{
    [TestFixture]
    public class CustomFormatsFixture : CoreTest
    {
        private static List<CustomFormats.CustomFormat> _customFormats { get; set; }

        public static void GivenCustomFormats(params CustomFormats.CustomFormat[] formats)
        {
            _customFormats = formats.ToList();
        }

        public static List<ProfileFormatItem> GetSampleFormatItems(params string[] allowed)
        {
            return _customFormats.Select(f => new ProfileFormatItem {Format = f, Allowed = allowed.Contains(f.Name)}).ToList();
        }
    }
}
