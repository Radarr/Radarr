import { createSelector } from 'reselect';
import appState from 'App/State/AppState';
import Movie from 'Movie/Movie';
import { createMovieSelectorForHook } from './createMovieSelector';

function createMovieQualityProfileSelector(movieId: number) {
  return createSelector(
    (state: appState) => state.settings.qualityProfiles.items,
    createMovieSelectorForHook(movieId),
    (qualityProfiles, movie = {} as Movie) => {
      return qualityProfiles.find(
        (profile) => profile.id === movie.qualityProfileId
      );
    }
  );
}

export default createMovieQualityProfileSelector;
