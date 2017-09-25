import { combineReducers } from 'redux';
import { enableBatching } from 'redux-batched-actions';
import { routerReducer } from 'react-router-redux';
import app, { defaultState as defaultappState } from './appReducers';
import addArtist, { defaultState as defaultAddSeriesState } from './addArtistReducers';
import importArtist, { defaultState as defaultImportArtistState } from './importArtistReducers';
import series, { defaultState as defaultArtistState } from './artistReducers';
import artistIndex, { defaultState as defaultArtistIndexState } from './artistIndexReducers';
import artistEditor, { defaultState as defaultArtistEditorState } from './artistEditorReducers';
import albumStudio, { defaultState as defaultAlbumStudioState } from './albumStudioReducers';
import calendar, { defaultState as defaultCalendarState } from './calendarReducers';
import history, { defaultState as defaultHistoryState } from './historyReducers';
import queue, { defaultState as defaultQueueState } from './queueReducers';
import blacklist, { defaultState as defaultBlacklistState } from './blacklistReducers';
import episodes, { defaultState as defaultEpisodesState } from './episodeReducers';
import tracks, { defaultState as defaultTracksState } from './trackReducers';
import trackFiles, { defaultState as defaultTrackFilesState } from './trackFileReducers';
import albumHistory, { defaultState as defaultAlbumHistoryState } from './albumHistoryReducers';
import releases, { defaultState as defaultReleasesState } from './releaseReducers';
import wanted, { defaultState as defaultWantedState } from './wantedReducers';
import settings, { defaultState as defaultSettingsState } from './settingsReducers';
import system, { defaultState as defaultSystemState } from './systemReducers';
import commands, { defaultState as defaultCommandsState } from './commandReducers';
import paths, { defaultState as defaultPathsState } from './pathReducers';
import tags, { defaultState as defaultTagsState } from './tagReducers';
import captcha, { defaultState as defaultCaptchaState } from './captchaReducers';
import oAuth, { defaultState as defaultOAuthState } from './oAuthReducers';
import interactiveImport, { defaultState as defaultInteractiveImportState } from './interactiveImportReducers';
import rootFolders, { defaultState as defaultRootFoldersState } from './rootFolderReducers';
import organizePreview, { defaultState as defaultOrganizePreviewState } from './organizePreviewReducers';

export const defaultState = {
  app: defaultappState,
  addArtist: defaultAddSeriesState,
  importArtist: defaultImportArtistState,
  series: defaultArtistState,
  artistIndex: defaultArtistIndexState,
  artistEditor: defaultArtistEditorState,
  albumStudio: defaultAlbumStudioState,
  calendar: defaultCalendarState,
  history: defaultHistoryState,
  queue: defaultQueueState,
  blacklist: defaultBlacklistState,
  episodes: defaultEpisodesState,
  tracks: defaultTracksState,
  trackFiles: defaultTrackFilesState,
  albumHistory: defaultAlbumHistoryState,
  releases: defaultReleasesState,
  wanted: defaultWantedState,
  settings: defaultSettingsState,
  system: defaultSystemState,
  commands: defaultCommandsState,
  paths: defaultPathsState,
  tags: defaultTagsState,
  captcha: defaultCaptchaState,
  oAuth: defaultOAuthState,
  interactiveImport: defaultInteractiveImportState,
  rootFolders: defaultRootFoldersState,
  organizePreview: defaultOrganizePreviewState
};

export default enableBatching(combineReducers({
  app,
  addArtist,
  importArtist,
  series,
  artistIndex,
  artistEditor,
  albumStudio,
  calendar,
  history,
  queue,
  blacklist,
  episodes,
  tracks,
  trackFiles,
  albumHistory,
  releases,
  wanted,
  settings,
  system,
  commands,
  paths,
  tags,
  captcha,
  oAuth,
  interactiveImport,
  rootFolders,
  organizePreview,
  routing: routerReducer
}));
