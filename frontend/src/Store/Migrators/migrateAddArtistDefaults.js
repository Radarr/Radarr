import { get } from 'lodash';
import monitorOptions from 'Utilities/Artist/monitorOptions';

export default function migrateAddArtistDefaults(persistedState) {
  const monitor = get(persistedState, 'addArtist.defaults.monitor');

  if (!monitor) {
    return;
  }

  if (!monitorOptions.find((option) => option.key === monitor)) {
    persistedState.addArtist.defaults.monitor = monitorOptions[0].key;
  }
}
