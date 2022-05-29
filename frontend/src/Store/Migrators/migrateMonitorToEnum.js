import get from 'lodash';

export default function migrateMonitorToEnum(persistedState) {
  const addMovie = get(persistedState, 'addMovie.defaults.monitor');
  const discoverMovie = get(persistedState, 'discoverMovie.defaults.monitor');

  if (addMovie != null) {
    if (addMovie) {
      persistedState.addMovie.defaults.monitor = 'movieOnly';
    } else {
      persistedState.addMovie.defaults.monitor = 'none';
    }
  }

  if (discoverMovie != null) {
    if (discoverMovie) {
      persistedState.discoverMovie.defaults.monitor = 'movieOnly';
    } else {
      persistedState.discoverMovie.defaults.monitor = 'none';
    }
  }
}
