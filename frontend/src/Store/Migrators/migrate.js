import migrateBlacklistToBlocklist from './migrateBlacklistToBlocklist';
import migrateMonitorToEnum from './migrateMonitorToEnum';
import migratePreDbToReleased from './migratePreDbToReleased';

export default function migrate(persistedState) {
  migrateBlacklistToBlocklist(persistedState);
  migratePreDbToReleased(persistedState);
  migrateMonitorToEnum(persistedState);
}
