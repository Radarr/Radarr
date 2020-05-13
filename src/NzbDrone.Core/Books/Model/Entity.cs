using System;
using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Books
{
    public abstract class Entity<T> : ModelBase, IEquatable<T>
        where T : Entity<T>
    {
        private static readonly MemberwiseEqualityComparer<T> _comparer =
            MemberwiseEqualityComparer<T>.ByProperties;

        public virtual void UseDbFieldsFrom(T other)
        {
            Id = other.Id;
        }

        public virtual void UseMetadataFrom(T other)
        {
        }

        public virtual void ApplyChanges(T other)
        {
        }

        public bool Equals(T other)
        {
            return _comparer.Equals(this as T, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as T);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this as T);
        }
    }
}
