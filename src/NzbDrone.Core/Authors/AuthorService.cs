using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Authors
{
    public interface IAuthorService
    {
        Author GetAuthor(int authorId);
        List<Author> GetAuthors(IEnumerable<int> authorIds);
        Author AddAuthor(Author newAuthor);
        List<Author> AddAuthors(List<Author> newAuthors);
        Author FindByName(string name);
        Author FindByForeignId(string foreignAuthorId);
        void DeleteAuthor(int authorId, bool deleteFiles);
        void DeleteAuthors(List<int> authorIds, bool deleteFiles);
        List<Author> GetAllAuthors();
        List<Author> GetMonitoredAuthors();
        Author UpdateAuthor(Author author);
        List<Author> UpdateAuthors(List<Author> authors);
        bool AuthorPathExists(string path);
    }

    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public Author GetAuthor(int authorId)
        {
            return _authorRepository.Get(authorId);
        }

        public List<Author> GetAuthors(IEnumerable<int> authorIds)
        {
            return _authorRepository.Get(authorIds).ToList();
        }

        public Author AddAuthor(Author newAuthor)
        {
            newAuthor.Added = DateTime.UtcNow;
            return _authorRepository.Insert(newAuthor);
        }

        public List<Author> AddAuthors(List<Author> newAuthors)
        {
            var now = DateTime.UtcNow;
            foreach (var author in newAuthors)
            {
                author.Added = now;
            }

            _authorRepository.InsertMany(newAuthors);
            return newAuthors;
        }

        public Author FindByName(string name)
        {
            return _authorRepository.FindByName(name);
        }

        public Author FindByForeignId(string foreignAuthorId)
        {
            return _authorRepository.FindByForeignId(foreignAuthorId);
        }

        public void DeleteAuthor(int authorId, bool deleteFiles)
        {
            _authorRepository.Delete(authorId);
        }

        public void DeleteAuthors(List<int> authorIds, bool deleteFiles)
        {
            _authorRepository.DeleteMany(authorIds);
        }

        public List<Author> GetAllAuthors()
        {
            return _authorRepository.All().ToList();
        }

        public List<Author> GetMonitoredAuthors()
        {
            return _authorRepository.GetMonitored();
        }

        public Author UpdateAuthor(Author author)
        {
            return _authorRepository.Update(author);
        }

        public List<Author> UpdateAuthors(List<Author> authors)
        {
            _authorRepository.UpdateMany(authors);
            return authors;
        }

        public bool AuthorPathExists(string path)
        {
            return _authorRepository.AuthorPathExists(path);
        }
    }
}
