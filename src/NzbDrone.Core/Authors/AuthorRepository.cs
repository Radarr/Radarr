using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authors
{
    public interface IAuthorRepository : IBasicRepository<Author>
    {
        Author FindByName(string name);
        Author FindByForeignId(string foreignAuthorId);
        List<Author> GetMonitored();
        bool AuthorPathExists(string path);
    }

    public class AuthorRepository : BasicRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Author FindByName(string name)
        {
            return Query(a => a.Name == name).FirstOrDefault();
        }

        public Author FindByForeignId(string foreignAuthorId)
        {
            return Query(a => a.ForeignAuthorId == foreignAuthorId).FirstOrDefault();
        }

        public List<Author> GetMonitored()
        {
            return Query(a => a.Monitored);
        }

        public bool AuthorPathExists(string path)
        {
            return Query(a => a.Path == path).Any();
        }
    }
}
