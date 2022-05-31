import _ from 'lodash';

export default function migratePreDbToReleased(persistedState) {
  const addMovie = _.get(persistedState, 'addMovie.defaults.minimumAvailability');
  const discoverMovie = _.get(persistedState, 'discoverMovie.defaults.minimumAvailability');

  if (!addMovie && !discoverMovie) {
    return;
  }

  if (addMovie === 'preDB') {
    persistedState.addMovie.defaults.minimumAvailability = 'released';
  }

  if (discoverMovie === 'preDB') {
    persistedState.discoverMovie.defaults.minimumAvailability = 'released';
  }
}
