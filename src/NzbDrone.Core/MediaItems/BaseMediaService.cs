using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaItems
{
    public interface IBaseMediaService<T> where T : ModelBase
    {
        T Get(int id);
        List<T> Get(IEnumerable<int> ids);
        PagingSpec<T> Paged(PagingSpec<T> pagingSpec);
        T Add(T item);
        List<T> AddMany(List<T> items);
        void Delete(int id, bool deleteFiles);
        void DeleteMany(List<int> ids, bool deleteFiles);
        List<T> GetAll();
        T Update(T item);
        List<T> UpdateMany(List<T> items);
    }

    public abstract class BaseMediaService<T> : IBaseMediaService<T> where T : ModelBase
    {
        protected abstract IBasicRepository<T> Repository { get; }
        protected virtual IEventAggregator EventAggregator => null;

        public T Get(int id)
        {
            return Repository.Get(id);
        }

        public List<T> Get(IEnumerable<int> ids)
        {
            return Repository.Get(ids).ToList();
        }

        public PagingSpec<T> Paged(PagingSpec<T> pagingSpec)
        {
            return Repository.GetPaged(pagingSpec);
        }

        public virtual T Add(T item)
        {
            SetAddedTimestamp(item);
            var inserted = Repository.Insert(item);
            OnItemAdded(Get(inserted.Id));
            return inserted;
        }

        public virtual List<T> AddMany(List<T> items)
        {
            var now = DateTime.UtcNow;
            foreach (var item in items)
            {
                SetAddedTimestamp(item, now);
            }

            Repository.InsertMany(items);
            OnItemsImported(items);
            return items;
        }

        public virtual void Delete(int id, bool deleteFiles)
        {
            var item = Repository.Get(id);
            Repository.Delete(id);
            OnItemDeleted(item, deleteFiles);
        }

        public virtual void DeleteMany(List<int> ids, bool deleteFiles)
        {
            var items = Repository.Get(ids).ToList();
            Repository.DeleteMany(ids);
            OnItemsDeleted(items, deleteFiles);
        }

        public List<T> GetAll()
        {
            return Repository.All().ToList();
        }

        public virtual T Update(T item)
        {
            var stored = Get(item.Id);
            var updated = Repository.Update(item);
            OnItemEdited(updated, stored);
            return updated;
        }

        public virtual List<T> UpdateMany(List<T> items)
        {
            Repository.UpdateMany(items);
            OnItemsBulkEdited(items);
            return items;
        }

        protected virtual void SetAddedTimestamp(T item, DateTime? timestamp = null)
        {
            var ts = timestamp ?? DateTime.UtcNow;

            if (item is MediaItem mediaItem)
            {
                mediaItem.Added = ts;
                return;
            }

            var addedProperty = item.GetType().GetProperty("Added");
            if (addedProperty != null && addedProperty.PropertyType == typeof(DateTime) && addedProperty.CanWrite)
            {
                addedProperty.SetValue(item, ts);
            }
        }

        protected virtual void OnItemAdded(T item) { }
        protected virtual void OnItemsImported(List<T> items) { }
        protected virtual void OnItemDeleted(T item, bool deleteFiles) { }
        protected virtual void OnItemsDeleted(List<T> items, bool deleteFiles) { }
        protected virtual void OnItemEdited(T updated, T stored) { }
        protected virtual void OnItemsBulkEdited(List<T> items) { }
    }
}
