import { createSelector } from 'reselect';

function createArtistSelector() {
  return createSelector(
    (state, { artistId }) => artistId,
    (state) => state.artist.itemMap,
    (state) => state.artist.items,
    (artistId, itemMap, allArtists) => {
      return allArtists[itemMap[artistId]];
    }
  );
}

export default createArtistSelector;
