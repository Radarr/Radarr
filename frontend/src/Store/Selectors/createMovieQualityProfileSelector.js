import { createSelector } from 'reselect';
import { createMovieSelectorForHook } from './createMovieSelector';

function createMovieQualityProfileSelector(movieId) {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createMovieSelectorForHook(movieId),
    (qualityProfiles, movie = {}) => {
      return qualityProfiles.find((profile) => {
        return profile.id === movie.qualityProfileId;
      });
    }
  );
}

export default createMovieQualityProfileSelector;
