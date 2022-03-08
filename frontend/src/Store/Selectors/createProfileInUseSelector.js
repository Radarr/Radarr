import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllMoviesSelector(),
    (state) => state.settings.importLists.items,
    (state) => state.movieCollections.items,
    (id, movies, lists, collections) => {
      if (!id) {
        return false;
      }

      if (_.some(movies, { [profileProp]: id }) || _.some(lists, { [profileProp]: id }) || _.some(collections, { [profileProp]: id })) {
        return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
