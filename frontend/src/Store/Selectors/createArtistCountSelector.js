import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createArtistCountSelector() {
  return createSelector(
    createAllArtistSelector(),
    (series) => {
      return series.length;
    }
  );
}

export default createArtistCountSelector;
