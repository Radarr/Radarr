import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createExistingArtistSelector() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    createAllArtistSelector(),
    (titleSlug, artist) => {
      return _.some(artist, { titleSlug });
    }
  );
}

export default createExistingArtistSelector;
