using System.Linq;
using NzbDrone.Core.Books;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForAuthor : IHousekeepingTask
    {
        private readonly IAuthorRepository _authorRepository;

        public UpdateCleanTitleForAuthor(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public void Clean()
        {
            var authors = _authorRepository.All().ToList();

            authors.ForEach(s =>
            {
                var cleanName = s.Name.CleanAuthorName();
                if (s.CleanName != cleanName)
                {
                    s.CleanName = cleanName;
                    _authorRepository.Update(s);
                }
            });
        }
    }
}
