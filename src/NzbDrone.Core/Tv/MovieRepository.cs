using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        Movie FindByTitle(string cleanTitle);
        Movie FindByTitle(string cleanTitle, int year);
        Movie FindByImdbId(string imdbid);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        public MovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool MoviePathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Movie FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Movie FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .AndWhere(s => s.Year == year)
                        .SingleOrDefault();
        }

        public Movie FindByImdbId(string imdbid)
        {
            return Query.Where(s => s.ImdbId == imdbid).SingleOrDefault();
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query.Where(m => m.MovieFileId == fileId).ToList();
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Movie { Id = episodeId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

    }
}