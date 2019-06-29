import * as addArtist from './addArtistActions';
import * as app from './appActions';
import * as blacklist from './blacklistActions';
import * as calendar from './calendarActions';
import * as captcha from './captchaActions';
import * as customFilters from './customFilterActions';
import * as commands from './commandActions';
import * as albums from './albumActions';
import * as trackFiles from './trackFileActions';
import * as albumHistory from './albumHistoryActions';
import * as history from './historyActions';
import * as importArtist from './importArtistActions';
import * as interactiveImportActions from './interactiveImportActions';
import * as oAuth from './oAuthActions';
import * as organizePreview from './organizePreviewActions';
import * as retagPreview from './retagPreviewActions';
import * as paths from './pathActions';
import * as providerOptions from './providerOptionActions';
import * as queue from './queueActions';
import * as releases from './releaseActions';
import * as rootFolders from './rootFolderActions';
import * as albumStudio from './albumStudioActions';
import * as artist from './artistActions';
import * as artistEditor from './artistEditorActions';
import * as artistHistory from './artistHistoryActions';
import * as artistIndex from './artistIndexActions';
import * as settings from './settingsActions';
import * as system from './systemActions';
import * as tags from './tagActions';
import * as tracks from './trackActions';
import * as wanted from './wantedActions';

export default [
  addArtist,
  app,
  blacklist,
  captcha,
  calendar,
  commands,
  customFilters,
  albums,
  trackFiles,
  albumHistory,
  history,
  importArtist,
  interactiveImportActions,
  oAuth,
  organizePreview,
  retagPreview,
  paths,
  providerOptions,
  queue,
  releases,
  rootFolders,
  albumStudio,
  artist,
  artistEditor,
  artistHistory,
  artistIndex,
  settings,
  system,
  tags,
  tracks,
  wanted
];
