import migrateBlacklistToBlocklist from './migrateBlacklistToBlocklist';
import migratePreDbToReleased from './migratePreDbToReleased';

export default function migrate(persistedState) {
  migrateBlacklistToBlocklist(persistedState);
  migratePreDbToReleased(persistedState);
}
