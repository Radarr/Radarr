import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllAuthorsSelector(),
    (state) => state.settings.importLists.items,
    (id, author, lists) => {
      if (!id) {
        return false;
      }

      if (_.some(author, { [profileProp]: id }) || _.some(lists, { [profileProp]: id })) {
        return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
