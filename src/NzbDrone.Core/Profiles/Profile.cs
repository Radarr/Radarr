﻿using System.Collections.Generic;
using System.Linq;
 using NzbDrone.Core.CustomFormats;
 using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles
{
    public class Profile : ModelBase
    {
        public Profile()
        {
            FormatItems = new List<ProfileFormatItem>();
        }

        public string Name { get; set; }
        public Quality Cutoff { get; set; }
        public List<ProfileQualityItem> Items { get; set; }
        public CustomFormat FormatCutoff { get; set; }
        public List<ProfileFormatItem> FormatItems { get; set; }
        public List<string> PreferredTags { get; set; }
        public Language Language { get; set; }

        public Quality LastAllowedQuality()
        {
            return Items.Last(q => q.Allowed).Quality;
        }
    }
}
