import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.metadataOptions';

//
// Actions Types

export const FETCH_METADATA_OPTIONS = 'settings/metadataOptions/fetchMetadataOptions';
export const SAVE_METADATA_OPTIONS = 'settings/metadataOptions/saveMetadataOptions';
export const SET_METADATA_OPTIONS_VALUE = 'settings/metadataOptions/setMetadataOptionsValue';

//
// Action Creators

export const fetchMetadataOptions = createThunk(FETCH_METADATA_OPTIONS);
export const saveMetadataOptions = createThunk(SAVE_METADATA_OPTIONS);
export const setMetadataOptionsValue = createAction(SET_METADATA_OPTIONS_VALUE, (payload) => {
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
    [FETCH_METADATA_OPTIONS]: createFetchHandler(section, '/config/metadata'),
    [SAVE_METADATA_OPTIONS]: createSaveHandler(section, '/config/metadata')
  },

  //
  // Reducers

  reducers: {
    [SET_METADATA_OPTIONS_VALUE]: createSetSettingValueReducer(section)
  }

};
