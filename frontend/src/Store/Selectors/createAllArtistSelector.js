import { createSelector } from 'reselect';

function createAllArtistSelector() {
  return createSelector(
    (state) => state.artist,
    (artist) => {
      return artist.items;
    }
  );
}

export default createAllArtistSelector;
