import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllMoviesSelector(),
    (id, movies) => {
      if (!id) {
        return false;
      }

      return _.some(movies, { [profileProp]: id });
    }
  );
}

export default createProfileInUseSelector;
