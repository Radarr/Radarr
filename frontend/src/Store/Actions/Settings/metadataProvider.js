import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.metadataProvider';

//
// Actions Types

export const FETCH_METADATA_PROVIDER = 'settings/metadataProvider/fetchMetadataProvider';
export const SET_METADATA_PROVIDER_VALUE = 'settings/metadataProvider/setMetadataProviderValue';
export const SAVE_METADATA_PROVIDER = 'settings/metadataProvider/saveMetadataProvider';

//
// Action Creators

export const fetchMetadataProvider = createThunk(FETCH_METADATA_PROVIDER);
export const saveMetadataProvider = createThunk(SAVE_METADATA_PROVIDER);
export const setMetadataProviderValue = createAction(SET_METADATA_PROVIDER_VALUE, (payload) => {
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
    [FETCH_METADATA_PROVIDER]: createFetchHandler(section, '/config/metadataProvider'),
    [SAVE_METADATA_PROVIDER]: createSaveHandler(section, '/config/metadataProvider')
  },

  //
  // Reducers

  reducers: {
    [SET_METADATA_PROVIDER_VALUE]: createSetSettingValueReducer(section)
  }

};
