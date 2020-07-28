import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.netImportExclusions';

//
// Actions Types

export const FETCH_NET_IMPORT_EXCLUSIONS = 'settings/netImportExclusions/fetchNetImportExclusions';
export const SAVE_NET_IMPORT_EXCLUSION = 'settings/netImportExclusions/saveNetImportExclusion';
export const DELETE_NET_IMPORT_EXCLUSION = 'settings/netImportExclusions/deleteNetImportExclusion';
export const SET_NET_IMPORT_EXCLUSION_VALUE = 'settings/netImportExclusions/setNetImportExclusionValue';

//
// Action Creators

export const fetchNetImportExclusions = createThunk(FETCH_NET_IMPORT_EXCLUSIONS);

export const saveNetImportExclusion = createThunk(SAVE_NET_IMPORT_EXCLUSION);
export const deleteNetImportExclusion = createThunk(DELETE_NET_IMPORT_EXCLUSION);

export const setNetImportExclusionValue = createAction(SET_NET_IMPORT_EXCLUSION_VALUE, (payload) => {
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
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_NET_IMPORT_EXCLUSIONS]: createFetchHandler(section, '/exclusions'),

    [SAVE_NET_IMPORT_EXCLUSION]: createSaveProviderHandler(section, '/exclusions'),
    [DELETE_NET_IMPORT_EXCLUSION]: createRemoveItemHandler(section, '/exclusions')
  },

  //
  // Reducers

  reducers: {
    [SET_NET_IMPORT_EXCLUSION_VALUE]: createSetSettingValueReducer(section)
  }

};
