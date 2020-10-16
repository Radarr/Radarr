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
        public Source Source { get; set; }
        public int Resolution { get; set; }
        public Modifier Modifier { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, Source source, int resolution, Modifier modifier = Modifier.NONE)
        {
            Id = id;
            Name = name;
            Source = source;
            Resolution = resolution;
            Modifier = modifier;
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
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

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

        // Unable to determine
        public static Quality Unknown => new Quality(0, "Unknown", Source.UNKNOWN, 0);

        // Pre-release
        public static Quality WORKPRINT => new Quality(24, "WORKPRINT", Source.WORKPRINT, 0); // new
        public static Quality CAM => new Quality(25, "CAM", Source.CAM, 0); // new
        public static Quality TELESYNC => new Quality(26, "TELESYNC", Source.TELESYNC, 0); // new
        public static Quality TELECINE => new Quality(27, "TELECINE", Source.TELECINE, 0); // new
        public static Quality DVDSCR => new Quality(28, "DVDSCR", Source.DVD, 480, Modifier.SCREENER); // new
        public static Quality REGIONAL => new Quality(29, "REGIONAL", Source.DVD, 480, Modifier.REGIONAL); // new

        // SD
        public static Quality SDTV => new Quality(1, "SDTV", Source.TV, 480);
        public static Quality DVD => new Quality(2, "DVD", Source.DVD, 480);
        public static Quality DVDR => new Quality(23, "DVD-R", Source.DVD, 480, Modifier.REMUX); // new

        // HDTV
        public static Quality HDTV720p => new Quality(4, "HDTV-720p", Source.TV, 720);
        public static Quality HDTV1080p => new Quality(9, "HDTV-1080p", Source.TV, 1080);
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p", Source.TV, 2160);

        // Web-DL
        public static Quality WEBDL480p => new Quality(8, "WEBDL-480p", Source.WEBDL, 480);
        public static Quality WEBDL720p => new Quality(5, "WEBDL-720p", Source.WEBDL, 720);
        public static Quality WEBDL1080p => new Quality(3, "WEBDL-1080p", Source.WEBDL, 1080);
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p", Source.WEBDL, 2160);

        // Bluray
        public static Quality Bluray480p => new Quality(20, "Bluray-480p", Source.BLURAY, 480); // new
        public static Quality Bluray576p => new Quality(21, "Bluray-576p", Source.BLURAY, 576); // new
        public static Quality Bluray720p => new Quality(6, "Bluray-720p", Source.BLURAY, 720);
        public static Quality Bluray1080p => new Quality(7, "Bluray-1080p", Source.BLURAY, 1080);
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p", Source.BLURAY, 2160);

        public static Quality Remux1080p => new Quality(30, "Remux-1080p", Source.BLURAY, 1080, Modifier.REMUX);
        public static Quality Remux2160p => new Quality(31, "Remux-2160p", Source.BLURAY, 2160, Modifier.REMUX);

        public static Quality BRDISK => new Quality(22, "BR-DISK", Source.BLURAY, 1080, Modifier.BRDISK); // new

        // Others
        public static Quality RAWHD => new Quality(10, "Raw-HD", Source.TV, 1080, Modifier.RAWHD);

        public static Quality WEBRip480p => new Quality(12, "WEBRip-480p", Source.WEBRIP, 480);
        public static Quality WEBRip720p => new Quality(14, "WEBRip-720p", Source.WEBRIP, 720);
        public static Quality WEBRip1080p => new Quality(15, "WEBRip-1080p", Source.WEBRIP, 1080);
        public static Quality WEBRip2160p => new Quality(17, "WEBRip-2160p", Source.WEBRIP, 2160);

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                WORKPRINT,
                CAM,
                TELESYNC,
                TELECINE,
                DVDSCR,
                REGIONAL,
                SDTV,
                DVD,
                DVDR,
                HDTV720p,
                HDTV1080p,
                HDTV2160p,
                WEBDL480p,
                WEBDL720p,
                WEBDL1080p,
                WEBDL2160p,
                WEBRip480p,
                WEBRip720p,
                WEBRip1080p,
                WEBRip2160p,
                Bluray480p,
                Bluray576p,
                Bluray720p,
                Bluray1080p,
                Bluray2160p,
                Remux1080p,
                Remux2160p,
                BRDISK,
                RAWHD
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WORKPRINT)   { Weight = 2,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.CAM)         { Weight = 3,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.TELESYNC)    { Weight = 4,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.TELECINE)    { Weight = 5,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.REGIONAL)    { Weight = 6,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVDSCR)      { Weight = 7,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.SDTV)        { Weight = 8,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVD)         { Weight = 9,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVDR)        { Weight = 10,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.WEBDL480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 480p" },
                new QualityDefinition(Quality.WEBRip480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 480p" },
                new QualityDefinition(Quality.Bluray480p)  { Weight = 12, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.Bluray576p)  { Weight = 13, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV720p)    { Weight = 14, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 720p" },
                new QualityDefinition(Quality.WEBRip720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 720p" },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 16, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV1080p)   { Weight = 17, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.WEBRip1080p)   { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 19, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux1080p)  { Weight = 20, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.HDTV2160p)   { Weight = 21, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.WEBRip2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 23, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux2160p)  { Weight = 24, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.BRDISK)      { Weight = 25, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.RAWHD)       { Weight = 26, MinSize = 0, MaxSize = null, PreferredSize = null }
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;
        public static Quality FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            var quality = AllLookup[id];

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", "id");
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
