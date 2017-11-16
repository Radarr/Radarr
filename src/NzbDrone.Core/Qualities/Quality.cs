using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        public static Quality Unknown => new Quality(0,  "Unknown");
        public static Quality MP3_192 => new Quality(1, "MP3-192");
        public static Quality MP3_VBR => new Quality(2, "MP3-VBR");
        public static Quality MP3_256 => new Quality(3, "MP3-256");
        public static Quality MP3_320 => new Quality(4, "MP3-320");
        public static Quality MP3_512 => new Quality(5, "MP3-512");
        public static Quality FLAC => new Quality(6, "FLAC");

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                MP3_192,
                MP3_VBR,
                MP3_256,
                MP3_320,
                MP3_512,
                FLAC,
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.MP3_192)        { Weight = 2,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.MP3_VBR)   { Weight = 3,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.MP3_256)         { Weight = 4,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.MP3_320)    { Weight = 5,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.MP3_512)    { Weight = 6,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.FLAC)   { Weight = 7,  MinSize = 0, MaxSize = null },
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;

        public static Quality FindById(int id)
        {
            if (id == 0) return Unknown;
            else if (id > AllLookup.Length)
            {
                throw new ArgumentException("ID does not match a known quality", nameof(id));
            }

            var quality = AllLookup[id];

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", nameof(id));
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
