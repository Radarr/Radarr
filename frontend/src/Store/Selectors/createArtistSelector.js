import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createArtistSelector() {
  return createSelector(
    (state, { artistId }) => artistId,
    createAllArtistSelector(),
    (artistId, allArtists) => {
      return allArtists.find((artist) => artist.id === artistId );
    }
  );
}

export default createArtistSelector;
