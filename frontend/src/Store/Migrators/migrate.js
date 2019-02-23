import migrateAddArtistDefaults from './migrateAddArtistDefaults';

export default function migrate(persistedState) {
  migrateAddArtistDefaults(persistedState);
}
