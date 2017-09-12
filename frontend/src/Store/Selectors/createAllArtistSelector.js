import { createSelector } from 'reselect';

function createAllArtistSelector() {
  return createSelector(
    (state) => state.series,
    (series) => {
      return series.items;
    }
  );
}

export default createAllArtistSelector;
