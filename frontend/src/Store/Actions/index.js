import * as addMovie from './addMovieActions';
import * as app from './appActions';
import * as blocklist from './blocklistActions';
import * as calendar from './calendarActions';
import * as captcha from './captchaActions';
import * as commands from './commandActions';
import * as customFilters from './customFilterActions';
import * as discoverMovie from './discoverMovieActions';
import * as extraFiles from './extraFileActions';
import * as history from './historyActions';
import * as importMovie from './importMovieActions';
import * as interactiveImportActions from './interactiveImportActions';
import * as movies from './movieActions';
import * as movieBlocklist from './movieBlocklistActions';
import * as movieCredits from './movieCreditsActions';
import * as movieFiles from './movieFileActions';
import * as movieHistory from './movieHistoryActions';
import * as movieIndex from './movieIndexActions';
import * as oAuth from './oAuthActions';
import * as organizePreview from './organizePreviewActions';
import * as paths from './pathActions';
import * as providerOptions from './providerOptionActions';
import * as queue from './queueActions';
import * as releases from './releaseActions';
import * as rootFolders from './rootFolderActions';
import * as settings from './settingsActions';
import * as system from './systemActions';
import * as tags from './tagActions';

export default [
  addMovie,
  app,
  blocklist,
  calendar,
  captcha,
  commands,
  customFilters,
  discoverMovie,
  movieFiles,
  extraFiles,
  history,
  importMovie,
  interactiveImportActions,
  oAuth,
  organizePreview,
  paths,
  providerOptions,
  queue,
  releases,
  rootFolders,
  movies,
  movieBlocklist,
  movieHistory,
  movieIndex,
  movieCredits,
  settings,
  system,
  tags
];
