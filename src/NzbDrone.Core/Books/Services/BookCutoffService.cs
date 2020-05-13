using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Books
{
    public interface IBookCutoffService
    {
        PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec);
    }

    public class BookCutoffService : IBookCutoffService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IProfileService _profileService;

        public BookCutoffService(IBookRepository bookRepository, IProfileService profileService)
        {
            _bookRepository = bookRepository;
            _profileService = profileService;
        }

        public PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _profileService.All();

            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoffIndex = profile.GetIndex(profile.Cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            return _bookRepository.BooksWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }
    }
}
