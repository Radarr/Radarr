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

        // Unable to determine
        public static Quality Unknown => new Quality(0, "Unknown");

        // Pre-release
        public static Quality WORKPRINT => new Quality(24, "WORKPRINT"); // new
        public static Quality CAM => new Quality(25, "CAM"); // new
        public static Quality TELESYNC => new Quality(26, "TELESYNC"); // new
        public static Quality TELECINE => new Quality(27, "TELECINE"); // new
        public static Quality DVDSCR => new Quality(28, "DVDSCR"); // new
        public static Quality REGIONAL => new Quality(29, "REGIONAL"); // new

        // SD
        public static Quality SDTV => new Quality(1, "SDTV");
        public static Quality DVD => new Quality(2, "DVD");
        public static Quality DVDR => new Quality(23, "DVD-R"); // new

        // HDTV
        public static Quality HDTV720p => new Quality(4, "HDTV-720p");
        public static Quality HDTV1080p => new Quality(9, "HDTV-1080p");
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p");

        // Web-DL
        public static Quality WEBDL480p => new Quality(8, "WEBDL-480p");
        public static Quality WEBDL720p => new Quality(5, "WEBDL-720p");
        public static Quality WEBDL1080p => new Quality(3, "WEBDL-1080p");
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p");

        // Bluray
        public static Quality Bluray480p => new Quality(20, "Bluray-480p"); // new
        public static Quality Bluray576p => new Quality(21, "Bluray-576p"); // new
        public static Quality Bluray720p => new Quality(6, "Bluray-720p");
        public static Quality Bluray1080p => new Quality(7, "Bluray-1080p");
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p");

        public static Quality Remux1080p => new Quality(30, "Remux-1080p");
        public static Quality Remux2160p => new Quality(31, "Remux-2160p");

        public static Quality BRDISK => new Quality(22, "BR-DISK"); // new

        // Others
        public static Quality RAWHD => new Quality(10, "Raw-HD");

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
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static Quality FindById(int id)
        {
            if (id == 0) return Unknown;

            var quality = AllLookup[id];

            if (quality == null)
                throw new ArgumentException("ID does not match a known quality", "id");
                        
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