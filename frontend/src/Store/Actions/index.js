import * as app from './appActions';
import * as author from './authorActions';
import * as authorEditor from './authorEditorActions';
import * as authorHistory from './authorHistoryActions';
import * as authorIndex from './authorIndexActions';
import * as blacklist from './blacklistActions';
import * as books from './bookActions';
import * as bookFiles from './bookFileActions';
import * as bookHistory from './bookHistoryActions';
import * as bookStudio from './bookshelfActions';
import * as calendar from './calendarActions';
import * as captcha from './captchaActions';
import * as commands from './commandActions';
import * as customFilters from './customFilterActions';
import * as history from './historyActions';
import * as interactiveImportActions from './interactiveImportActions';
import * as oAuth from './oAuthActions';
import * as organizePreview from './organizePreviewActions';
import * as paths from './pathActions';
import * as providerOptions from './providerOptionActions';
import * as queue from './queueActions';
import * as releases from './releaseActions';
import * as retagPreview from './retagPreviewActions';
import * as search from './searchActions';
import * as series from './seriesActions';
import * as settings from './settingsActions';
import * as system from './systemActions';
import * as tags from './tagActions';
import * as wanted from './wantedActions';

export default [
  app,
  blacklist,
  captcha,
  calendar,
  commands,
  customFilters,
  books,
  bookFiles,
  bookHistory,
  history,
  interactiveImportActions,
  oAuth,
  organizePreview,
  retagPreview,
  paths,
  providerOptions,
  queue,
  releases,
  bookStudio,
  author,
  authorEditor,
  authorHistory,
  authorIndex,
  series,
  search,
  settings,
  system,
  tags,
  wanted
];
