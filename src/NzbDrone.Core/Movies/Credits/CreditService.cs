using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Credits
{
    public interface ICreditService
    {
        List<Credit> GetAllCreditsForMovieMetadata(int movieMetadataId);
        Credit AddCredit(Credit credit, MovieMetadata movie);
        List<Credit> AddCredits(List<Credit> credits, MovieMetadata movie);
        Credit GetById(int id);
        List<Credit> GetAllCredits();
        List<Credit> UpdateCredits(List<Credit> credits, MovieMetadata movie);
    }

    public class CreditService : ICreditService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly ICreditRepository _creditRepo;
        private readonly Logger _logger;

        public CreditService(ICreditRepository creditRepo, Logger logger)
        {
            _creditRepo = creditRepo;
            _logger = logger;
        }

        public List<Credit> GetAllCreditsForMovieMetadata(int movieMetadataId)
        {
            return _creditRepo.FindByMovieMetadataId(movieMetadataId).ToList();
        }

        public Credit AddCredit(Credit credit, MovieMetadata movie)
        {
            credit.MovieMetadataId = movie.Id;
            return _creditRepo.Insert(credit);
        }

        public List<Credit> AddCredits(List<Credit> credits, MovieMetadata movie)
        {
            credits.ForEach(t => t.MovieMetadataId = movie.Id);
            _creditRepo.InsertMany(credits);
            return credits;
        }

        public Credit GetById(int id)
        {
            return _creditRepo.Get(id);
        }

        public List<Credit> GetAllCredits()
        {
            return _creditRepo.All().ToList();
        }

        public void RemoveTitle(Credit credit)
        {
            _creditRepo.Delete(credit);
        }

        public List<Credit> UpdateCredits(List<Credit> credits, MovieMetadata movieMetadata)
        {
            var movieMetadataId = movieMetadata.Id;

            // First update the movie ids so we can correlate them later.
            credits.ForEach(t => t.MovieMetadataId = movieMetadataId);

            // Should never have multiple credits with same credit_id, but check to ensure in case TMDB is on fritz
            var dupeFreeCredits = credits.DistinctBy(m => m.CreditTmdbId).ToList();

            var existingCredits = _creditRepo.FindByMovieMetadataId(movieMetadataId);

            var updateList = new List<Credit>();
            var addList = new List<Credit>();
            var upToDateCount = 0;

            foreach (var credit in dupeFreeCredits)
            {
                var existingCredit = existingCredits.FirstOrDefault(x => x.CreditTmdbId == credit.CreditTmdbId);

                if (existingCredit != null)
                {
                    existingCredits.Remove(existingCredit);

                    credit.UseDbFieldsFrom(existingCredit);

                    if (!credit.Equals(existingCredit))
                    {
                        updateList.Add(credit);
                    }
                    else
                    {
                        upToDateCount++;
                    }
                }
                else
                {
                    addList.Add(credit);
                }
            }

            _creditRepo.DeleteMany(existingCredits);
            _creditRepo.UpdateMany(updateList);
            _creditRepo.InsertMany(addList);

            _logger.Debug("[{0}] {1} credits up to date; Updating {2}, Adding {3}, Deleting {4} entries.", movieMetadata.Title, upToDateCount, updateList.Count, addList.Count, existingCredits.Count);

            return credits;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO handle metadata deletions and not movie deletions
            _creditRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
