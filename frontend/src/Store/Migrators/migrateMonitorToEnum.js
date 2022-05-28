import get from 'lodash';

export default function migrateMonitorToEnum(persistedState) {
  const addMovie = get(persistedState, 'addMovie.defaults.monitor');
  const discoverMovie = get(persistedState, 'discoverMovie.defaults.monitor');

  if (!addMovie && !discoverMovie) {
    return;
  }

  if (addMovie === true) {
    persistedState.addMovie.defaults.monitor = 'movieOnly';
  }

  if (discoverMovie === true) {
    persistedState.discoverMovie.defaults.monitor = 'movieOnly';
  }

  if (addMovie === false) {
    persistedState.addMovie.defaults.monitor = 'none';
  }

  if (discoverMovie === false) {
    persistedState.discoverMovie.defaults.monitor = 'none';
  }
}
