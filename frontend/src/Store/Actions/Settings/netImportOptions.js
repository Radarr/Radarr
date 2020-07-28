import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.netImportOptions';

//
// Actions Types

export const FETCH_NET_IMPORT_OPTIONS = 'settings/netImportOptions/fetchNetImportOptions';
export const SAVE_NET_IMPORT_OPTIONS = 'settings/netImportOptions/saveNetImportOptions';
export const SET_NET_IMPORT_OPTIONS_VALUE = 'settings/netImportOptions/setNetImportOptionsValue';

//
// Action Creators

export const fetchNetImportOptions = createThunk(FETCH_NET_IMPORT_OPTIONS);
export const saveNetImportOptions = createThunk(SAVE_NET_IMPORT_OPTIONS);
export const setNetImportOptionsValue = createAction(SET_NET_IMPORT_OPTIONS_VALUE, (payload) => {
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
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_NET_IMPORT_OPTIONS]: createFetchHandler(section, '/config/netimport'),
    [SAVE_NET_IMPORT_OPTIONS]: createSaveHandler(section, '/config/netimport')
  },

  //
  // Reducers

  reducers: {
    [SET_NET_IMPORT_OPTIONS_VALUE]: createSetSettingValueReducer(section)
  }

};
