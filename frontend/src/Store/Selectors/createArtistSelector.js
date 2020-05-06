import { createSelector } from 'reselect';

function createArtistSelector() {
  return createSelector(
    (state, { authorId }) => authorId,
    (state) => state.artist.itemMap,
    (state) => state.artist.items,
    (authorId, itemMap, allArtists) => {
      return allArtists[itemMap[authorId]];
    }
  );
}

export default createArtistSelector;
