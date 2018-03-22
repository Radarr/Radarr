using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionRepository : IBasicRepository<QualityDefinition>
    {
    }

    public class QualityDefinitionRepository : BasicRepository<QualityDefinition>, IQualityDefinitionRepository
    {
        public QualityDefinitionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public IEnumerable<QualityDefinition> All()
        {
            var text = Query.Where(t => true).OrderBy(t => t.Id).BuildQuery();
            var asdf = text + "asdf";
            return Query.Where(d => true).ToList(); //Only sort builder does joins so hack :/
        }

        protected override QueryBuilder<QualityDefinition> AddJoinQueries(QueryBuilder<QualityDefinition> baseQuery)
        {
            var query = base.AddJoinQueries(baseQuery);
            query = query.Join<QualityDefinition, QualityDefinition>(JoinType.Left,
                d => d.ParentQualityDefinition, (d, parent) => d.ParentQualityDefinitionId == parent.Id);
            return query;
        }
    }
}
