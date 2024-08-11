export type ListSyncLevel =
  | 'disabled'
  | 'logOnly'
  | 'keepAndUnmonitor'
  | 'removeAndKeep'
  | 'removeAndDelete';

export default interface ImportListOptionsSettings {
  listSyncLevel: ListSyncLevel;
}
