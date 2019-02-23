import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createArtistCountSelector() {
  return createSelector(
    createAllArtistSelector(),
    (artists) => {
      return artists.length;
    }
  );
}

export default createArtistCountSelector;
