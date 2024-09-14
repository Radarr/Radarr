import _ from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { MovieCreditType } from 'typings/MovieCredit';

function createMovieCreditsSelector(movieCreditType: MovieCreditType) {
  return createSelector(
    (state: AppState) => state.movieCredits.items,
    (movieCredits) => {
      const credits = movieCredits.filter(
        ({ type }) => type === movieCreditType
      );

      const sortedCredits = credits.sort((a, b) => a.order - b.order);

      return {
        items: _.uniqBy(sortedCredits, 'personName'),
      };
    }
  );
}

export default createMovieCreditsSelector;
