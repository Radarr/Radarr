import _ from 'lodash';

export default function migrateMonitorToEnum(persistedState) {
  const addMovie = _.get(persistedState, 'addMovie.defaults.monitor');
  const discoverMovie = _.get(persistedState, 'discoverMovie.defaults.monitor');

  if (addMovie) {
    if (addMovie === 'true') {
      persistedState.addMovie.defaults.monitor = 'movieOnly';
    }

    if (addMovie === 'false') {
      persistedState.addMovie.defaults.monitor = 'none';
    }
  }

  if (discoverMovie) {
    if (discoverMovie === 'true') {
      persistedState.discoverMovie.defaults.monitor = 'movieOnly';
    }

    if (discoverMovie === 'false') {
      persistedState.discoverMovie.defaults.monitor = 'none';
    }
  }
}
