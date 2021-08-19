import _, { get } from 'lodash';

export default function migrateBlacklistToBlocklist(persistedState) {
  const blocklist = get(persistedState, 'blacklist');

  if (!blocklist) {
    return;
  }

  persistedState.blocklist = blocklist;
  _.remove(persistedState, 'blacklist');
}
