import { createAction } from 'redux-actions';
import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import customFormats from './Settings/customFormats';
import customFormatSpecifications from './Settings/customFormatSpecifications';
import delayProfiles from './Settings/delayProfiles';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';
import general from './Settings/general';
import indexerOptions from './Settings/indexerOptions';
import indexers from './Settings/indexers';
import languages from './Settings/languages';
import mediaManagement from './Settings/mediaManagement';
import metadata from './Settings/metadata';
import metadataOptions from './Settings/metadataOptions';
import naming from './Settings/naming';
import namingExamples from './Settings/namingExamples';
import netImportExclusions from './Settings/netImportExclusions';
import netImportOptions from './Settings/netImportOptions';
import netImports from './Settings/netImports';
import notifications from './Settings/notifications';
import qualityDefinitions from './Settings/qualityDefinitions';
import qualityProfiles from './Settings/qualityProfiles';
import remotePathMappings from './Settings/remotePathMappings';
import restrictions from './Settings/restrictions';
import ui from './Settings/ui';

export * from './Settings/customFormatSpecifications.js';
export * from './Settings/customFormats';
export * from './Settings/delayProfiles';
export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';
export * from './Settings/general';
export * from './Settings/indexerOptions';
export * from './Settings/indexers';
export * from './Settings/languages';
export * from './Settings/netImportExclusions';
export * from './Settings/netImportOptions';
export * from './Settings/netImports';
export * from './Settings/mediaManagement';
export * from './Settings/metadata';
export * from './Settings/metadataOptions';
export * from './Settings/naming';
export * from './Settings/namingExamples';
export * from './Settings/notifications';
export * from './Settings/qualityDefinitions';
export * from './Settings/qualityProfiles';
export * from './Settings/remotePathMappings';
export * from './Settings/restrictions';
export * from './Settings/ui';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false,

  customFormatSpecifications: customFormatSpecifications.defaultState,
  customFormats: customFormats.defaultState,
  delayProfiles: delayProfiles.defaultState,
  downloadClients: downloadClients.defaultState,
  downloadClientOptions: downloadClientOptions.defaultState,
  general: general.defaultState,
  indexerOptions: indexerOptions.defaultState,
  indexers: indexers.defaultState,
  languages: languages.defaultState,
  netImportExclusions: netImportExclusions.defaultState,
  netImportOptions: netImportOptions.defaultState,
  netImports: netImports.defaultState,
  mediaManagement: mediaManagement.defaultState,
  metadata: metadata.defaultState,
  metadataOptions: metadataOptions.defaultState,
  naming: naming.defaultState,
  namingExamples: namingExamples.defaultState,
  notifications: notifications.defaultState,
  qualityDefinitions: qualityDefinitions.defaultState,
  qualityProfiles: qualityProfiles.defaultState,
  remotePathMappings: remotePathMappings.defaultState,
  restrictions: restrictions.defaultState,
  ui: ui.defaultState
};

export const persistState = [
  'settings.advancedSettings'
];

//
// Actions Types

export const TOGGLE_ADVANCED_SETTINGS = 'settings/toggleAdvancedSettings';

//
// Action Creators

export const toggleAdvancedSettings = createAction(TOGGLE_ADVANCED_SETTINGS);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...customFormatSpecifications.actionHandlers,
  ...customFormats.actionHandlers,
  ...delayProfiles.actionHandlers,
  ...downloadClients.actionHandlers,
  ...downloadClientOptions.actionHandlers,
  ...general.actionHandlers,
  ...indexerOptions.actionHandlers,
  ...indexers.actionHandlers,
  ...languages.actionHandlers,
  ...netImportExclusions.actionHandlers,
  ...netImportOptions.actionHandlers,
  ...netImports.actionHandlers,
  ...mediaManagement.actionHandlers,
  ...metadata.actionHandlers,
  ...metadataOptions.actionHandlers,
  ...naming.actionHandlers,
  ...namingExamples.actionHandlers,
  ...notifications.actionHandlers,
  ...qualityDefinitions.actionHandlers,
  ...qualityProfiles.actionHandlers,
  ...remotePathMappings.actionHandlers,
  ...restrictions.actionHandlers,
  ...ui.actionHandlers
});

//
// Reducers

export const reducers = createHandleActions({

  [TOGGLE_ADVANCED_SETTINGS]: (state, { payload }) => {
    return Object.assign({}, state, { advancedSettings: !state.advancedSettings });
  },

  ...customFormatSpecifications.reducers,
  ...customFormats.reducers,
  ...delayProfiles.reducers,
  ...downloadClients.reducers,
  ...downloadClientOptions.reducers,
  ...general.reducers,
  ...indexerOptions.reducers,
  ...indexers.reducers,
  ...languages.reducers,
  ...netImportExclusions.reducers,
  ...netImportOptions.reducers,
  ...netImports.reducers,
  ...mediaManagement.reducers,
  ...metadata.reducers,
  ...metadataOptions.reducers,
  ...naming.reducers,
  ...namingExamples.reducers,
  ...notifications.reducers,
  ...qualityDefinitions.reducers,
  ...qualityProfiles.reducers,
  ...remotePathMappings.reducers,
  ...restrictions.reducers,
  ...ui.reducers

}, defaultState, section);
