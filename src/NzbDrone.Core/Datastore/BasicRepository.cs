using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Hosting;
using Marr.Data;
using Marr.Data.QGen;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Get(int id);
        IEnumerable<TModel> Get(IEnumerable<int> ids);
        TModel SingleOrDefault();
        TModel Insert(TModel model);
        TModel Update(TModel model);
        TModel Upsert(TModel model);
        void Delete(int id);
        void Delete(TModel model);
        void InsertMany(IList<TModel> model);
        void UpdateMany(IList<TModel> model);
        void DeleteMany(List<TModel> model);
        void Purge(bool vacuum = false);
        bool HasItems();
        void DeleteMany(IEnumerable<int> ids);
        void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties);
        TModel Single();
        PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly IDatabase _database;
        private readonly IEventAggregator _eventAggregator;

        protected IDataMapper DataMapper()
        {
            return _database.GetDataMapper();
        }

        public BasicRepository(IDatabase database, IEventAggregator eventAggregator)
        {
            _database = database;
            _eventAggregator = eventAggregator;
        }


        protected T Query<T>(Func<QueryBuilder<TModel>, T> finalizeQuery)
        {
            using (var mapper = DataMapper())
            {
                var query = AddJoinQueries(mapper.Query<TModel>());
                return finalizeQuery(query);
            }
        }

        protected void Delete(Expression<Func<TModel, bool>> filter)
        {
            using (var db = DataMapper())
            {
                db.Delete(filter);
            }
        }

        public IEnumerable<TModel> All()
        {
            return Query((q => q.ToList()));
        }

        public int Count()
        {
            using (var db = DataMapper())
            {
                return db.Query<TModel>().GetRowCount();
            }
        }

        public TModel Get(int id)
        {
            TModel model = Query(q => q.Where(c => c.Id == id).SingleOrDefault());

            if (model == null)
            {
                throw new ModelNotFoundException(typeof(TModel), id);
            }

            return model;
        }

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            var query = string.Format("Id IN ({0})", string.Join(",", idList));
            var result = Query(q => q.Where(m => m.Id.In(idList)).ToList());
            //var result = Query.Where(query).ToList();

            if (result.Count != idList.Count())
            {
                throw new ApplicationException("Expected query to return {0} rows but returned {1}.".Inject(idList.Count(), result.Count));
            }

            return result;
        }

        public TModel SingleOrDefault()
        {
            return All().SingleOrDefault();
        }

        public TModel Single()
        {
            return All().Single();
        }

        public TModel Insert(TModel model)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID " + model.Id);
            }

            using (var db = DataMapper())
            {
                db.Insert(model);
            }

            ModelCreated(model);

            return model;
        }

        public TModel Update(TModel model)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using (var db = DataMapper())
            {
                db.Update(model, c => c.Id == model.Id);
            }

            ModelUpdated(model);

            return model;
        }

        public void Delete(TModel model)
        {
            Delete(model.Id);
        }

        public void InsertMany(IList<TModel> models)
        {
            using (var unitOfWork = new UnitOfWork(() => DataMapper()))
            {
                unitOfWork.BeginTransaction(IsolationLevel.ReadCommitted);

                foreach (var model in models)
                {
                    unitOfWork.DB.Insert(model);
                }

                unitOfWork.Commit();
            }
        }

        public void UpdateMany(IList<TModel> models)
        {
            using (var unitOfWork = new UnitOfWork(() => DataMapper()))
            {
                unitOfWork.BeginTransaction(IsolationLevel.ReadCommitted);

                foreach (var model in models)
                {
                    var localModel = model;

                    if (model.Id == 0)
                    {
                        throw new InvalidOperationException("Can't update model with ID 0");
                    }

                    unitOfWork.DB.Update(model, c => c.Id == localModel.Id);
                }

                unitOfWork.Commit();
            }
        }

        public void DeleteMany(List<TModel> models)
        {
            DeleteMany(models.Select(m => m.Id));
        }

        public TModel Upsert(TModel model)
        {
            if (model.Id == 0)
            {
                Insert(model);
                return model;
            }
            Update(model);
            return model;
        }

        public void Delete(int id)
        {
            using (var db = DataMapper())
            {
                db.Delete<TModel>(c => c.Id == id);
            }
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            using (var unitOfWork = new UnitOfWork(() => DataMapper()))
            {
                unitOfWork.BeginTransaction(IsolationLevel.ReadCommitted);

                foreach (var id in ids)
                {
                    var localId = id;

                    unitOfWork.DB.Delete<TModel>(c => c.Id == localId);
                }

                unitOfWork.Commit();
            }
        }

        public void Purge(bool vacuum = false)
        {
            using (var db = DataMapper())
            {
                db.Delete<TModel>(c => c.Id > -1);
            }
            if (vacuum)
            {
                Vacuum();
            }
        }

        protected void Vacuum()
        {
            _database.Vacuum();
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        public void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to updated model without ID");
            }

            using (var db = DataMapper())
            {
                db.Update<TModel>()
                    .Where(c => c.Id == model.Id)
                    .ColumnsIncluding(properties)
                    .Entity(model)
                    .Execute();   
            }

            ModelUpdated(model);
        }

        public virtual PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec)
        {
            pagingSpec.Records = Query(q => GetPagedQuery(q, pagingSpec).Skip(pagingSpec.PagingOffset())
                .Take(pagingSpec.PageSize).ToList());
            pagingSpec.TotalRecords = Query(q => GetPagedQuery(q, pagingSpec).GetRowCount());

            return pagingSpec;
        }

        protected virtual SortBuilder<TModel> GetPagedQuery(QueryBuilder<TModel> query, PagingSpec<TModel> pagingSpec)
        {
            var filterExpressions = pagingSpec.FilterExpressions;
            var sortQuery = query.Where(filterExpressions.FirstOrDefault());

            if (filterExpressions.Count > 1)
            {
                // Start at the second item for the AndWhere clauses
                for (var i = 1; i < filterExpressions.Count; i++)
                {
                    sortQuery.AndWhere(filterExpressions[i]);
                }
            }

            return sortQuery.OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                           .Skip(pagingSpec.PagingOffset())
                           .Take(pagingSpec.PageSize);
        }

        protected void ModelCreated(TModel model)
        {
            PublishModelEvent(model, ModelAction.Created);
        }

        protected void ModelUpdated(TModel model)
        {
            PublishModelEvent(model, ModelAction.Updated);
        }

        protected void ModelDeleted(TModel model)
        {
            PublishModelEvent(model, ModelAction.Deleted);
        }

        private void PublishModelEvent(TModel model, ModelAction action)
        {
            if (PublishModelEvents)
            {
                _eventAggregator.PublishEvent(new ModelEvent<TModel>(model, action));
            }
        }

        protected virtual QueryBuilder<TActual> AddJoinQueries<TActual>(QueryBuilder<TActual> baseQuery)
        {
            return baseQuery;
        }

        protected virtual bool PublishModelEvents => false;
    }
}
