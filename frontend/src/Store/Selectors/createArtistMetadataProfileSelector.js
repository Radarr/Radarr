import { createSelector } from 'reselect';
import createArtistSelector from './createArtistSelector';

function createArtistMetadataProfileSelector() {
  return createSelector(
    (state) => state.settings.metadataProfiles.items,
    createArtistSelector(),
    (metadataProfiles, artist = {}) => {
      return metadataProfiles.find((profile) => {
        return profile.id === artist.metadataProfileId;
      });
    }
  );
}

export default createArtistMetadataProfileSelector;
