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
        public static Quality MP3_VBR => new Quality(2, "MP3-VBR-V0");
        public static Quality MP3_256 => new Quality(3, "MP3-256");
        public static Quality MP3_320 => new Quality(4, "MP3-320");
        public static Quality MP3_160 => new Quality(5, "MP3-160");
        public static Quality FLAC => new Quality(6, "FLAC");
        public static Quality ALAC => new Quality(7, "ALAC");
        public static Quality MP3_VBR_V2 => new Quality(8, "MP3-VBR-V2");
        public static Quality AAC_192 => new Quality(9, "AAC-192");
        public static Quality AAC_256 => new Quality(10, "AAC-256");
        public static Quality AAC_320 => new Quality(11, "AAC-320");
        public static Quality AAC_VBR => new Quality(12, "AAC-VBR");
        public static Quality WAV => new Quality(13, "WAV");
        public static Quality VORBIS_Q10 => new Quality(14, "OGG Vorbis Q10");
        public static Quality VORBIS_Q9 => new Quality(15, "OGG Vorbis Q9");
        public static Quality VORBIS_Q8 => new Quality(16, "OGG Vorbis Q8");
        public static Quality VORBIS_Q7 => new Quality(17, "OGG Vorbis Q7");
        public static Quality VORBIS_Q6 => new Quality(18, "OGG Vorbis Q6");
        public static Quality VORBIS_Q5 => new Quality(19, "OGG Vorbis Q5");
        public static Quality WMA => new Quality(20, "WMA");
        public static Quality FLAC_24 => new Quality(21, "FLAC 24bit");
        public static Quality MP3_128 => new Quality(22, "MP3-128");
        public static Quality MP3_096 => new Quality(23, "MP3-96"); // For Current Files Only
        public static Quality MP3_080 => new Quality(24, "MP3-80"); // For Current Files Only
        public static Quality MP3_064 => new Quality(25, "MP3-64"); // For Current Files Only
        public static Quality MP3_056 => new Quality(26, "MP3-56"); // For Current Files Only
        public static Quality MP3_048 => new Quality(27, "MP3-48"); // For Current Files Only
        public static Quality MP3_040 => new Quality(28, "MP3-40"); // For Current Files Only
        public static Quality MP3_032 => new Quality(29, "MP3-32"); // For Current Files Only
        public static Quality MP3_024 => new Quality(30, "MP3-24"); // For Current Files Only
        public static Quality MP3_016 => new Quality(31, "MP3-16"); // For Current Files Only
        public static Quality MP3_008 => new Quality(32, "MP3-8"); // For Current Files Only
        public static Quality MP3_112 => new Quality(33, "MP3-112"); // For Current Files Only
        public static Quality MP3_224 => new Quality(34, "MP3-224"); // For Current Files Only
        public static Quality APE => new Quality(35, "APE");
        public static Quality WAVPACK => new Quality(36, "WavPack");

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                MP3_008,
                MP3_016,
                MP3_024,
                MP3_032,
                MP3_040,
                MP3_048,
                MP3_056,
                MP3_064,
                MP3_080,
                MP3_096,
                MP3_112,
                MP3_128,
                MP3_160,
                MP3_192,
                MP3_224,
                MP3_VBR,
                MP3_256,
                MP3_320,
                MP3_VBR_V2,
                AAC_192,
                AAC_256,
                AAC_320,
                AAC_VBR,
                WMA,
                VORBIS_Q10,
                VORBIS_Q9,
                VORBIS_Q8,
                VORBIS_Q7,
                VORBIS_Q6,
                VORBIS_Q5,
                ALAC,
                FLAC,
                APE,
                WAVPACK,
                FLAC_24,
                WAV
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1, MinSize = 0, MaxSize = 350, GroupWeight = 1},
                new QualityDefinition(Quality.MP3_008)     { Weight = 2, MinSize = 0, MaxSize = 10, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_016)     { Weight = 3, MinSize = 0, MaxSize = 20, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_024)     { Weight = 4, MinSize = 0, MaxSize = 30, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_032)     { Weight = 5, MinSize = 0, MaxSize = 40, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_040)     { Weight = 6, MinSize = 0, MaxSize = 45, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_048)     { Weight = 7, MinSize = 0, MaxSize = 55, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_056)     { Weight = 8, MinSize = 0, MaxSize = 65, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_064)     { Weight = 9, MinSize = 0, MaxSize = 75, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_080)     { Weight = 10, MinSize = 0, MaxSize = 95, GroupName = "Trash Quality Lossy", GroupWeight = 2 },
                new QualityDefinition(Quality.MP3_096)     { Weight = 11, MinSize = 0, MaxSize = 110, GroupName = "Poor Quality Lossy", GroupWeight = 3 },
                new QualityDefinition(Quality.MP3_112)     { Weight = 12, MinSize = 0, MaxSize = 125, GroupName = "Poor Quality Lossy", GroupWeight = 3 },
                new QualityDefinition(Quality.MP3_128)     { Weight = 13, MinSize = 0, MaxSize = 140, GroupName = "Poor Quality Lossy", GroupWeight = 3 },
                new QualityDefinition(Quality.VORBIS_Q5)   { Weight = 14, MinSize = 0, MaxSize = 175, GroupName = "Poor Quality Lossy", GroupWeight = 3 },
                new QualityDefinition(Quality.MP3_160)     { Weight = 14, MinSize = 0, MaxSize = 175, GroupName = "Poor Quality Lossy", GroupWeight = 3 },
                new QualityDefinition(Quality.MP3_192)     { Weight = 15, MinSize = 0, MaxSize = 210, GroupName = "Low Quality Lossy", GroupWeight = 4 },
                new QualityDefinition(Quality.VORBIS_Q6)   { Weight = 15, MinSize = 0, MaxSize = 210, GroupName = "Low Quality Lossy", GroupWeight = 4 },
                new QualityDefinition(Quality.AAC_192)     { Weight = 15, MinSize = 0, MaxSize = 210, GroupName = "Low Quality Lossy", GroupWeight = 4 },
                new QualityDefinition(Quality.WMA)         { Weight = 15, MinSize = 0, MaxSize = 350, GroupName = "Low Quality Lossy", GroupWeight = 4 },
                new QualityDefinition(Quality.MP3_224)     { Weight = 16, MinSize = 0, MaxSize = 245, GroupName = "Low Quality Lossy", GroupWeight = 4 },
                new QualityDefinition(Quality.VORBIS_Q7)   { Weight = 17, MinSize = 0, MaxSize = 245, GroupName = "Mid Quality Lossy", GroupWeight = 5 },
                new QualityDefinition(Quality.MP3_VBR_V2)  { Weight = 18, MinSize = 0, MaxSize = 280, GroupName = "Mid Quality Lossy", GroupWeight = 5 },
                new QualityDefinition(Quality.MP3_256)     { Weight = 18, MinSize = 0, MaxSize = 280, GroupName = "Mid Quality Lossy", GroupWeight = 5 },
                new QualityDefinition(Quality.VORBIS_Q8)   { Weight = 18, MinSize = 0, MaxSize = 280, GroupName = "Mid Quality Lossy", GroupWeight = 5 },
                new QualityDefinition(Quality.AAC_256)     { Weight = 18, MinSize = 0, MaxSize = 280, GroupName = "Mid Quality Lossy", GroupWeight = 5 },
                new QualityDefinition(Quality.MP3_VBR)     { Weight = 19, MinSize = 0, MaxSize = 350, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.AAC_VBR)     { Weight = 19, MinSize = 0, MaxSize = 350, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.MP3_320)     { Weight = 20, MinSize = 0, MaxSize = 350, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.VORBIS_Q9)   { Weight = 20, MinSize = 0, MaxSize = 350, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.AAC_320)     { Weight = 20, MinSize = 0, MaxSize = 350, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.VORBIS_Q10)  { Weight = 21, MinSize = 0, MaxSize = 550, GroupName = "High Quality Lossy", GroupWeight = 6 },
                new QualityDefinition(Quality.ALAC)        { Weight = 22, MinSize = 0, MaxSize = null, GroupName = "Lossless", GroupWeight = 7 },
                new QualityDefinition(Quality.FLAC)        { Weight = 22, MinSize = 0, MaxSize = null, GroupName = "Lossless", GroupWeight = 7 },
                new QualityDefinition(Quality.APE)         { Weight = 22, MinSize = 0, MaxSize = null, GroupName = "Lossless", GroupWeight = 7 },
                new QualityDefinition(Quality.WAVPACK)     { Weight = 22, MinSize = 0, MaxSize = null, GroupName = "Lossless", GroupWeight = 7 },
                new QualityDefinition(Quality.FLAC_24)     { Weight = 23, MinSize = 0, MaxSize = null, GroupName = "Lossless", GroupWeight = 7 },
                new QualityDefinition(Quality.WAV)         { Weight = 24, MinSize = 0, MaxSize = null, GroupWeight = 8}
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
