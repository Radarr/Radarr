import { createSelector } from 'reselect';
import createArtistSelector from './createArtistSelector';

function createArtistQualityProfileSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createArtistSelector(),
    (qualityProfiles, artist = {}) => {
      return qualityProfiles.find((profile) => {
        return profile.id === artist.qualityProfileId;
      });
    }
  );
}

export default createArtistQualityProfileSelector;
