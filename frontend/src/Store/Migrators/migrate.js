import migrateAddAuthorDefaults from './migrateAddAuthorDefaults';

export default function migrate(persistedState) {
  migrateAddAuthorDefaults(persistedState);
}
