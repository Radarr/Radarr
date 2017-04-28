using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        bool ArtistPathExists(string path);
        Artist FindByTitle(string cleanTitle);
        Artist FindByItunesId(int iTunesId);
    }

    public class ArtistRepository : IArtistRepository
    {
        public IEnumerable<Artist> All()
        {
            throw new NotImplementedException();
        }

        public bool ArtistPathExists(string path)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public void Delete(Artist model)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public void DeleteMany(List<Artist> model)
        {
            throw new NotImplementedException();
        }

        public Artist FindByItunesId(int iTunesId)
        {
            throw new NotImplementedException();
        }

        public Artist FindByTitle(string cleanTitle)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Artist> Get(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public Artist Get(int id)
        {
            throw new NotImplementedException();
        }

        public PagingSpec<Artist> GetPaged(PagingSpec<Artist> pagingSpec)
        {
            throw new NotImplementedException();
        }

        public bool HasItems()
        {
            throw new NotImplementedException();
        }

        public Artist Insert(Artist model)
        {
            throw new NotImplementedException();
        }

        public void InsertMany(IList<Artist> model)
        {
            throw new NotImplementedException();
        }

        public void Purge(bool vacuum = false)
        {
            throw new NotImplementedException();
        }

        public void SetFields(Artist model, params Expression<Func<Artist, object>>[] properties)
        {
            throw new NotImplementedException();
        }

        public Artist Single()
        {
            throw new NotImplementedException();
        }

        public Artist SingleOrDefault()
        {
            throw new NotImplementedException();
        }

        public Artist Update(Artist model)
        {
            throw new NotImplementedException();
        }

        public void UpdateMany(IList<Artist> model)
        {
            throw new NotImplementedException();
        }

        public Artist Upsert(Artist model)
        {
            throw new NotImplementedException();
        }
    }
}
