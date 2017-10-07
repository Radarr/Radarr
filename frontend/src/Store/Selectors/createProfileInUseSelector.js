import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllArtistSelector(),
    (id, artist) => {
      if (!id) {
        return false;
      }

      return _.some(artist, { [profileProp]: id });
    }
  );
}

export default createProfileInUseSelector;
