import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from 'Store/Actions/Creators/createSaveProviderHandler';
import createTestAllProvidersHandler from 'Store/Actions/Creators/createTestAllProvidersHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from 'Store/Actions/Creators/createTestProviderHandler';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';

//
// Variables

const section = 'settings.netImports';

//
// Actions Types

export const FETCH_NET_IMPORTS = 'settings/netImports/fetchNetImports';
export const FETCH_NET_IMPORT_SCHEMA = 'settings/netImports/fetchNetImportSchema';
export const SELECT_NET_IMPORT_SCHEMA = 'settings/netImports/selectNetImportSchema';
export const SET_NET_IMPORT_VALUE = 'settings/netImports/setNetImportValue';
export const SET_NET_IMPORT_FIELD_VALUE = 'settings/netImports/setNetImportFieldValue';
export const SAVE_NET_IMPORT = 'settings/netImports/saveNetImport';
export const CANCEL_SAVE_NET_IMPORT = 'settings/netImports/cancelSaveNetImport';
export const DELETE_NET_IMPORT = 'settings/netImports/deleteNetImport';
export const TEST_NET_IMPORT = 'settings/netImports/testNetImport';
export const CANCEL_TEST_NET_IMPORT = 'settings/netImports/cancelTestNetImport';
export const TEST_ALL_NET_IMPORT = 'settings/netImports/testAllNetImport';

//
// Action Creators

export const fetchNetImports = createThunk(FETCH_NET_IMPORTS);
export const fetchNetImportSchema = createThunk(FETCH_NET_IMPORT_SCHEMA);
export const selectNetImportSchema = createAction(SELECT_NET_IMPORT_SCHEMA);

export const saveNetImport = createThunk(SAVE_NET_IMPORT);
export const cancelSaveNetImport = createThunk(CANCEL_SAVE_NET_IMPORT);
export const deleteNetImport = createThunk(DELETE_NET_IMPORT);
export const testNetImport = createThunk(TEST_NET_IMPORT);
export const cancelTestNetImport = createThunk(CANCEL_TEST_NET_IMPORT);
export const testAllNetImport = createThunk(TEST_ALL_NET_IMPORT);

export const setNetImportValue = createAction(SET_NET_IMPORT_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setNetImportFieldValue = createAction(SET_NET_IMPORT_FIELD_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isSchemaFetching: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    isTestingAll: false,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_NET_IMPORTS]: createFetchHandler(section, '/netimport'),
    [FETCH_NET_IMPORT_SCHEMA]: createFetchSchemaHandler(section, '/netimport/schema'),

    [SAVE_NET_IMPORT]: createSaveProviderHandler(section, '/netimport'),
    [CANCEL_SAVE_NET_IMPORT]: createCancelSaveProviderHandler(section),
    [DELETE_NET_IMPORT]: createRemoveItemHandler(section, '/netimport'),
    [TEST_NET_IMPORT]: createTestProviderHandler(section, '/netimport'),
    [CANCEL_TEST_NET_IMPORT]: createCancelTestProviderHandler(section),
    [TEST_ALL_NET_IMPORT]: createTestAllProvidersHandler(section, '/netimport')
  },

  //
  // Reducers

  reducers: {
    [SET_NET_IMPORT_VALUE]: createSetSettingValueReducer(section),
    [SET_NET_IMPORT_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_NET_IMPORT_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {

        return selectedSchema;
      });
    }
  }

};
