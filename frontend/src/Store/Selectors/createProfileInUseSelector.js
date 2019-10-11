import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllMoviesSelector(),
    (state) => state.settings.netImports.items,
    (id, movies, lists) => {
      if (!id) {
        return false;
      }

      if (_.some(movies, { [profileProp]: id }) || _.some(lists, { [profileProp]: id })) {
        return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
