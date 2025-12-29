using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public interface IMusicFileService
    {
        MusicFile GetMusicFile(int id);
        List<MusicFile> GetMusicFiles(IEnumerable<int> ids);
        List<MusicFile> GetFilesByTrackId(int trackId);
        List<MusicFile> GetFilesByAlbumId(int albumId);
        List<MusicFile> GetFilesByTrackIds(List<int> trackIds);
        List<MusicFile> GetFilesByAlbumIds(List<int> albumIds);
        MusicFile Update(MusicFile musicFile);
        List<MusicFile> Update(List<MusicFile> musicFiles);
        void Delete(MusicFile musicFile);
        void Delete(List<MusicFile> musicFiles);
    }

    public class MusicFileService : IMusicFileService
    {
        private readonly IMusicFileRepository _musicFileRepository;

        public MusicFileService(IMusicFileRepository musicFileRepository)
        {
            _musicFileRepository = musicFileRepository;
        }

        public MusicFile GetMusicFile(int id)
        {
            return _musicFileRepository.Get(id);
        }

        public List<MusicFile> GetMusicFiles(IEnumerable<int> ids)
        {
            return _musicFileRepository.Get(ids).ToList();
        }

        public List<MusicFile> GetFilesByTrackId(int trackId)
        {
            return _musicFileRepository.GetFilesByTrackId(trackId);
        }

        public List<MusicFile> GetFilesByAlbumId(int albumId)
        {
            return _musicFileRepository.GetFilesByAlbumId(albumId);
        }

        public List<MusicFile> GetFilesByTrackIds(List<int> trackIds)
        {
            return trackIds.SelectMany(id => _musicFileRepository.GetFilesByTrackId(id)).ToList();
        }

        public List<MusicFile> GetFilesByAlbumIds(List<int> albumIds)
        {
            return albumIds.SelectMany(id => _musicFileRepository.GetFilesByAlbumId(id)).ToList();
        }

        public MusicFile Update(MusicFile musicFile)
        {
            return _musicFileRepository.Update(musicFile);
        }

        public List<MusicFile> Update(List<MusicFile> musicFiles)
        {
            _musicFileRepository.UpdateMany(musicFiles);
            return musicFiles;
        }

        public void Delete(MusicFile musicFile)
        {
            _musicFileRepository.Delete(musicFile);
        }

        public void Delete(List<MusicFile> musicFiles)
        {
            _musicFileRepository.DeleteMany(musicFiles);
        }
    }
}
