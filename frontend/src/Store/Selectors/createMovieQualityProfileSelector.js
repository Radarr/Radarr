import { createSelector } from 'reselect';
import createMovieSelector from './createMovieSelector';

function createMovieQualityProfileSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createMovieSelector(),
    (qualityProfiles, movie) => {
      return qualityProfiles.find((profile) => {
        return profile.id === movie.qualityProfileId;
      });
    }
  );
}

export default createMovieQualityProfileSelector;
