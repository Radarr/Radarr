import migrateBlacklistToBlocklist from './migrateBlacklistToBlocklist';

export default function migrate(persistedState) {
  migrateBlacklistToBlocklist(persistedState);
}
