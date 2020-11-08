import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.importExclusions';

//
// Actions Types

export const FETCH_IMPORT_EXCLUSIONS = 'settings/importExclusions/fetchImportExclusions';
export const SAVE_IMPORT_EXCLUSION = 'settings/importExclusions/saveImportExclusion';
export const DELETE_IMPORT_EXCLUSION = 'settings/importExclusions/deleteImportExclusion';
export const SET_IMPORT_EXCLUSION_VALUE = 'settings/importExclusions/setImportExclusionValue';

//
// Action Creators

export const fetchImportExclusions = createThunk(FETCH_IMPORT_EXCLUSIONS);

export const saveImportExclusion = createThunk(SAVE_IMPORT_EXCLUSION);
export const deleteImportExclusion = createThunk(DELETE_IMPORT_EXCLUSION);

export const setImportExclusionValue = createAction(SET_IMPORT_EXCLUSION_VALUE, (payload) => {
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
    [FETCH_IMPORT_EXCLUSIONS]: createFetchHandler(section, '/exclusions'),

    [SAVE_IMPORT_EXCLUSION]: createSaveProviderHandler(section, '/exclusions'),
    [DELETE_IMPORT_EXCLUSION]: createRemoveItemHandler(section, '/exclusions')
  },

  //
  // Reducers

  reducers: {
    [SET_IMPORT_EXCLUSION_VALUE]: createSetSettingValueReducer(section)
  }

};
