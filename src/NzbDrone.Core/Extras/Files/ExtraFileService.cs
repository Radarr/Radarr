using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileService<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        List<TExtraFile> GetFilesByAuthor(int authorId);
        List<TExtraFile> GetFilesByBookFile(int bookFileId);
        TExtraFile FindByPath(string path);
        void Upsert(TExtraFile extraFile);
        void Upsert(List<TExtraFile> extraFiles);
        void Delete(int id);
        void DeleteMany(IEnumerable<int> ids);
    }

    public abstract class ExtraFileService<TExtraFile> : IExtraFileService<TExtraFile>,
                                                         IHandleAsync<AuthorDeletedEvent>,
                                                         IHandle<BookFileDeletedEvent>
        where TExtraFile : ExtraFile, new()
    {
        private readonly IExtraFileRepository<TExtraFile> _repository;
        private readonly IAuthorService _authorService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly Logger _logger;

        public ExtraFileService(IExtraFileRepository<TExtraFile> repository,
                                IAuthorService authorService,
                                IDiskProvider diskProvider,
                                IRecycleBinProvider recycleBinProvider,
                                Logger logger)
        {
            _repository = repository;
            _authorService = authorService;
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _logger = logger;
        }

        public List<TExtraFile> GetFilesByAuthor(int authorId)
        {
            return _repository.GetFilesByAuthor(authorId);
        }

        public List<TExtraFile> GetFilesByBookFile(int bookFileId)
        {
            return _repository.GetFilesByBookFile(bookFileId);
        }

        public TExtraFile FindByPath(string path)
        {
            return _repository.FindByPath(path);
        }

        public void Upsert(TExtraFile extraFile)
        {
            Upsert(new List<TExtraFile> { extraFile });
        }

        public void Upsert(List<TExtraFile> extraFiles)
        {
            extraFiles.ForEach(m =>
            {
                m.LastUpdated = DateTime.UtcNow;

                if (m.Id == 0)
                {
                    m.Added = m.LastUpdated;
                }
            });

            _repository.InsertMany(extraFiles.Where(m => m.Id == 0).ToList());
            _repository.UpdateMany(extraFiles.Where(m => m.Id > 0).ToList());
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            _repository.DeleteMany(ids);
        }

        public void HandleAsync(AuthorDeletedEvent message)
        {
            _logger.Debug("Deleting Extra from database for author: {0}", message.Author);
            _repository.DeleteForAuthor(message.Author.Id);
        }

        public void Handle(BookFileDeletedEvent message)
        {
            var bookFile = message.BookFile;

            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing track file from DB as part of cleanup routine, not deleting extra files from disk.");
            }
            else
            {
                var author = bookFile.Author.Value;

                foreach (var extra in _repository.GetFilesByBookFile(bookFile.Id))
                {
                    var path = Path.Combine(author.Path, extra.RelativePath);

                    if (_diskProvider.FileExists(path))
                    {
                        // Send to the recycling bin so they can be recovered if necessary
                        var subfolder = _diskProvider.GetParentFolder(author.Path).GetRelativePath(_diskProvider.GetParentFolder(path));
                        _recycleBinProvider.DeleteFile(path, subfolder);
                    }
                }
            }

            _logger.Debug("Deleting Extra from database for track file: {0}", bookFile);
            _repository.DeleteForBookFile(bookFile.Id);
        }
    }
}
