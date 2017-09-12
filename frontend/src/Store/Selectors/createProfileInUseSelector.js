import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllArtistSelector(),
    (id, series) => {
      if (!id) {
        return false;
      }

      return _.some(series, { [profileProp]: id });
    }
  );
}

export default createProfileInUseSelector;
