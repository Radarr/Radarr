import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Movie from 'Movie/Movie';
import { createMovieSelectorForHook } from './createMovieSelector';

function createMovieQualityProfileSelector(movieId: number) {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    createMovieSelectorForHook(movieId),
    (qualityProfiles, movie = {} as Movie) => {
      return qualityProfiles.find(
        (profile) => profile.id === movie.qualityProfileId
      );
    }
  );
}

export default createMovieQualityProfileSelector;
