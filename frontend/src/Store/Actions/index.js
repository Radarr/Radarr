import * as addMovie from './addMovieActions';
import * as app from './appActions';
import * as blacklist from './blacklistActions';
import * as calendar from './calendarActions';
import * as captcha from './captchaActions';
import * as customFilters from './customFilterActions';
import * as devices from './deviceActions';
import * as commands from './commandActions';
import * as movieFiles from './movieFileActions';
import * as history from './historyActions';
import * as importMovie from './importMovieActions';
import * as interactiveImportActions from './interactiveImportActions';
import * as oAuth from './oAuthActions';
import * as organizePreview from './organizePreviewActions';
import * as paths from './pathActions';
import * as queue from './queueActions';
import * as releases from './releaseActions';
import * as rootFolders from './rootFolderActions';
import * as movies from './movieActions';
import * as movieEditor from './movieEditorActions';
import * as movieHistory from './movieHistoryActions';
import * as movieIndex from './movieIndexActions';
import * as settings from './settingsActions';
import * as system from './systemActions';
import * as tags from './tagActions';

export default [
  addMovie,
  app,
  blacklist,
  calendar,
  captcha,
  commands,
  customFilters,
  devices,
  movieFiles,
  history,
  importMovie,
  interactiveImportActions,
  oAuth,
  organizePreview,
  paths,
  queue,
  releases,
  rootFolders,
  movies,
  movieEditor,
  movieHistory,
  movieIndex,
  settings,
  system,
  tags
];
