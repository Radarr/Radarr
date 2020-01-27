using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForMovie(int movieId);
        void DeleteForMovieFile(int movieFileId);
        List<TExtraFile> GetFilesByMovie(int movieId);
        List<TExtraFile> GetFilesByMovieFile(int movieFileId);
        TExtraFile FindByPath(string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForMovie(int movieId)
        {
            Delete(movieId);
        }

        public void DeleteForMovieFile(int movieFileId)
        {
            Delete(c => c.MovieFileId == movieFileId);
        }

        public List<TExtraFile> GetFilesByMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<TExtraFile> GetFilesByMovieFile(int movieFileId)
        {
            return Query(x => x.MovieFileId == movieFileId);
        }

        public TExtraFile FindByPath(string path)
        {
            return Query(x => x.RelativePath == path).SingleOrDefault();
        }
    }
}
