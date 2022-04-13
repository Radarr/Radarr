using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForMovies(List<int> movieIds);
        void DeleteForMovieFile(int movieFileId);
        List<TExtraFile> GetFilesByMovie(int movieId);
        List<TExtraFile> GetFilesByMovieFile(int movieFileId);
        TExtraFile FindByPath(int movieId, string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        public void DeleteForMovieFile(int movieFileId)
        {
            Delete(x => x.MovieFileId == movieFileId);
        }

        public List<TExtraFile> GetFilesByMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<TExtraFile> GetFilesByMovieFile(int movieFileId)
        {
            return Query(x => x.MovieFileId == movieFileId);
        }

        public TExtraFile FindByPath(int movieId, string path)
        {
            return Query(c => c.MovieId == movieId && c.RelativePath == path).SingleOrDefault();
        }
    }
}
