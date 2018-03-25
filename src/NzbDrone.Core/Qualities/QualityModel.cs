using System;
using Newtonsoft.Json;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityModel : IEmbeddedDocument, IEquatable<QualityModel>
    {
        public Quality Quality { get; set; }

        public QualityDefinition QualityDefinition { get; set; }
        public Resolution Resolution { get; set; }
        public Source Source { get; set; }
        public Modifier Modifier { get; set; }
        public Revision Revision { get; set; }
        public string HardcodedSubs { get; set; }

        [JsonIgnore]
        public QualitySource QualitySource { get; set; }

        public QualityModel()
            : this(QualityDefinitionService.UnknownQualityDefinition, new Revision())
        {

        }

        public QualityModel(QualityDefinition quality, Revision revision = null)
        {
            Quality = quality.Quality ?? quality.ParentQualityDefinition.Quality;
            QualityDefinition = quality;
            Revision = revision ?? new Revision();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", QualityDefinition, Revision);
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

            return other.QualityDefinition.Id.Equals(QualityDefinition.Id) && other.Revision.Equals(Revision);
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
