﻿using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Credits
{
    public interface ICreditService
    {
        List<Credit> GetAllCreditsForMovie(int movieId);
        Credit AddCredit(Credit credit, Movie movie);
        List<Credit> AddCredits(List<Credit> credits, Movie movie);
        Credit GetById(int id);
        List<Credit> GetAllCredits();
        List<Credit> UpdateCredits(List<Credit> credits, Movie movie);
    }

    public class CreditService : ICreditService, IHandleAsync<MovieDeletedEvent>
    {
        private readonly ICreditRepository _creditRepo;

        public CreditService(ICreditRepository creditRepo)
        {
            _creditRepo = creditRepo;
        }

        public List<Credit> GetAllCreditsForMovie(int movieId)
        {
            return _creditRepo.FindByMovieId(movieId).ToList();
        }

        public Credit AddCredit(Credit credit, Movie movie)
        {
            credit.MovieId = movie.Id;
            return _creditRepo.Insert(credit);
        }

        public List<Credit> AddCredits(List<Credit> credits, Movie movie)
        {
            credits.ForEach(t => t.MovieId = movie.Id);
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

        public List<Credit> UpdateCredits(List<Credit> credits, Movie movie)
        {
            int movieId = movie.Id;

            // First update the movie ids so we can correlate them later.
            credits.ForEach(t => t.MovieId = movieId);

            // Now find credits to delete, update and insert.
            var existingCredits = _creditRepo.FindByMovieId(movieId);

            // Should never have multiple credits with same credit_id, but check to ensure incase TMDB is on fritz
            var dupeFreeCredits = credits.DistinctBy(m => m.CreditTmdbId).ToList();

            var insert = dupeFreeCredits.Where(t => !existingCredits.Any(c => c.CreditTmdbId == t.CreditTmdbId)).ToList();
            var update = existingCredits.Where(t => dupeFreeCredits.Any(c => c.CreditTmdbId == t.CreditTmdbId)).ToList();
            var delete = existingCredits.Where(t => !dupeFreeCredits.Any(c => c.CreditTmdbId == t.CreditTmdbId)).ToList();

            _creditRepo.DeleteMany(delete);
            _creditRepo.UpdateMany(update);
            _creditRepo.InsertMany(insert);

            return credits;
        }

        public void HandleAsync(MovieDeletedEvent message)
        {
            _creditRepo.DeleteMany(GetAllCreditsForMovie(message.Movie.Id));
        }
    }
}
