import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createArtistCountSelector() {
  return createSelector(
    createAllArtistSelector(),
    (state) => state.artist.error,
    (artists, error) => {
      return {
        count: artists.length,
        error
      };
    }
  );
}

export default createArtistCountSelector;
