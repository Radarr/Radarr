using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IAudiobookFileRepository : IBasicRepository<AudiobookFile>
    {
        List<AudiobookFile> GetFilesByAudiobook(int audiobookId);
        List<AudiobookFile> GetFilesByAudiobooks(IEnumerable<int> audiobookIds);
        void DeleteForAudiobooks(List<int> audiobookIds);
        List<AudiobookFile> GetFilesWithRelativePath(int audiobookId, string relativePath);
    }

    public class AudiobookFileRepository : BasicRepository<AudiobookFile>, IAudiobookFileRepository
    {
        public AudiobookFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<AudiobookFile> GetFilesByAudiobook(int audiobookId)
        {
            return Query(x => x.AudiobookId == audiobookId);
        }

        public List<AudiobookFile> GetFilesByAudiobooks(IEnumerable<int> audiobookIds)
        {
            return Query(x => audiobookIds.Contains(x.AudiobookId));
        }

        public void DeleteForAudiobooks(List<int> audiobookIds)
        {
            Delete(x => audiobookIds.Contains(x.AudiobookId));
        }

        public List<AudiobookFile> GetFilesWithRelativePath(int audiobookId, string relativePath)
        {
            return Query(c => c.AudiobookId == audiobookId && c.RelativePath == relativePath);
        }
    }
}
