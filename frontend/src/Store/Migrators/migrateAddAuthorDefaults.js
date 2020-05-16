import { get } from 'lodash';
import monitorOptions from 'Utilities/Author/monitorOptions';

export default function migrateAddAuthorDefaults(persistedState) {
  const monitor = get(persistedState, 'addAuthor.defaults.monitor');

  if (!monitor) {
    return;
  }

  if (!monitorOptions.find((option) => option.key === monitor)) {
    persistedState.addAuthor.defaults.monitor = monitorOptions[0].key;
  }
}
