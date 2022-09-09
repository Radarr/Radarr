import Tag from 'Settings/Tags/Tag';

export type ListSyncLevel =
  | 'disabled'
  | 'logOnly'
  | 'keepAndUnmonitor'
  | 'removeAndKeep'
  | 'removeAndDelete';

export type CleanLibraryTags = Tag;

export default interface ImportListOptionsSettings {
  listSyncLevel: ListSyncLevel;
  cleanLibraryTags: CleanLibraryTags;
}
