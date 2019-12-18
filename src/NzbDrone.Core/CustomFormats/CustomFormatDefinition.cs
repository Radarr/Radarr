using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormatDefinition : ModelBase
    {
        public string Name { get; set; }

        public List<FormatTag> FormatTags { get; set; }

        public static implicit operator CustomFormat(CustomFormatDefinition def) => new CustomFormat { Id = def.Id, Name = def.Name, FormatTags = def.FormatTags };
    }
}
