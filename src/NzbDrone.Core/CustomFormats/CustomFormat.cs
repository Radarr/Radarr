using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormat : ModelBase, IEquatable<CustomFormat>
    {
        public string Name { get; set; }

        public List<FormatTag> FormatTags { get; set; }

        public CustomFormat()
        {

        }

        public CustomFormat(string name, params string[] tags)
        {
            Name = name;
            FormatTags = tags.Select(t => new FormatTag(t)).ToList();
        }

        public static implicit operator CustomFormatDefinition(CustomFormat format) => new CustomFormatDefinition { Id = format.Id, Name = format.Name, FormatTags = format.FormatTags };

        public override string ToString()
        {
            return Name;
        }

        public static CustomFormat None => new CustomFormat("None");

        public bool Equals(CustomFormat other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return int.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomFormat) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public static class CustomFormatExtensions
    {
        public static string ToExtendedString(this IEnumerable<CustomFormat> formats)
        {
            return string.Join(", ", formats.Select(f => f.ToString()));
        }

        public static List<CustomFormat> WithNone(this IEnumerable<CustomFormat> formats)
        {
            var list = formats.ToList();
            if (list.Any()) return list;

            return new List<CustomFormat>{CustomFormat.None};
        }
    }
}
