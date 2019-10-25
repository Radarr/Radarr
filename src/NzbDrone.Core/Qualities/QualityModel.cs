using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityModel : IEmbeddedDocument, IEquatable<QualityModel>
    {
        public Quality Quality { get; set; }

        public List<CustomFormat> CustomFormats { get; set; }

        public Revision Revision { get; set; }

        public string HardcodedSubs { get; set; }

        [JsonIgnore]
        public QualityDetectionSource QualityDetectionSource { get; set; }

        public QualityModel()
            : this(Quality.Unknown, new Revision())
        {

        }

        public QualityModel(Quality quality, Revision revision = null)
        {
            Quality = quality;
            Revision = revision ?? new Revision();
            CustomFormats = new List<CustomFormat>();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Quality, Revision, CustomFormats.WithNone().ToExtendedString());
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Revision.GetHashCode();
                hash = hash * 23 + Quality.GetHashCode();
                return hash;
            }
        }

        public bool Equals(QualityModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.Quality.Id.Equals(Quality.Id) && other.Revision.Equals(Revision);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as QualityModel);
        }

        public static bool operator ==(QualityModel left, QualityModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(QualityModel left, QualityModel right)
        {
            return !Equals(left, right);
        }
    }
}
