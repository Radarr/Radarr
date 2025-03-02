using System;
using System.Collections.Generic;
using System.Linq;
using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ChangeTracker
{
    internal static class ChangeTracker<TSource>
        where TSource : ModelBase
    {
        private static MemberwiseEqualityComparer<TSource> _comparer;

        internal static void DetectChanges<TKey>(
            IEnumerable<TSource> source,
            IEnumerable<TSource> existing,
            Func<TSource, TKey> keySelector,
            out List<TSource> insert,
            out List<TSource> update,
            out List<TSource> delete)
            where TKey : class =>
        DetectChanges(source, existing, keySelector, null, out insert, out update, out delete);

        internal static void DetectChanges<TKey>(
            IEnumerable<TSource> source,
            IEnumerable<TSource> existing,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> keyComparer,
            out List<TSource> insert,
            out List<TSource> update,
            out List<TSource> delete)
            where TKey : class
        {
            keyComparer ??= EqualityComparer<TKey>.Default;
            _comparer ??= MemberwiseEqualityComparer<TSource>.ByProperties;

            insert = new List<TSource>();
            update = new List<TSource>();
            var existingClean = existing.ToList();
            foreach (var src in source)
            {
                // Try to find from current DB stored entities if one match
                var foundDb = existing.FirstOrDefault(t =>

                    // Optimization: Use HashCode first, and then Equality comparer
                    keyComparer.GetHashCode(keySelector(src)) == keyComparer.GetHashCode(keySelector(t)) &&
                    keyComparer.Equals(keySelector(src), keySelector(t)));

                // Does not exist, to insert
                if (foundDb == null)
                {
                    insert.Add(src);
                }
                else
                {
                    // Remove to only keep MovieTranslation that should be removed
                    existingClean.Remove(foundDb);

                    // Exists, deep comparison to check if a property change
                    if (!_comparer.Equals(src, foundDb))
                    {
                        // At least one property changed, update
                        // Set the Id to the entity to update
                        src.Id = foundDb.Id;
                        update.Add(src);
                    }
                }
            }

            // The only elements that remains in current DB stored entities
            // are the one not matching any of the new entities
            // They should be removed
            delete = existingClean;
        }
    }
}
