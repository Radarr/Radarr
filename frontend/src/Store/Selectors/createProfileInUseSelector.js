import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllArtistSelector(),
    (state) => state.settings.importLists.items,
    (id, artist, lists) => {
      if (!id) {
        return false;
      }

      if (_.some(artist, { [profileProp]: id }) || _.some(lists, { [profileProp]: id })) {
        return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
