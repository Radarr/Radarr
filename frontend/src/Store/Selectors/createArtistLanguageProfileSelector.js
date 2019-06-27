import { createSelector } from 'reselect';
import createArtistSelector from './createArtistSelector';

function createArtistLanguageProfileSelector() {
  return createSelector(
    (state) => state.settings.languageProfiles.items,
    createArtistSelector(),
    (languageProfiles, artist = {}) => {
      return languageProfiles.find((profile) => {
        return profile.id === artist.languageProfileId;
      });
    }
  );
}

export default createArtistLanguageProfileSelector;
