import { createSelector } from 'reselect';
import createMovieSelector from './createMovieSelector';

function createMovieQualityProfileSelector(movieId) {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createMovieSelector(movieId),
    (qualityProfiles, movie = {}) => {
      return qualityProfiles.find((profile) => {
        return profile.id === movie.qualityProfileId;
      });
    }
  );
}

export default createMovieQualityProfileSelector;
