import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.qualityProfiles';

//
// Actions Types

export const FETCH_QUALITY_PROFILES = 'settings/qualityProfiles/fetchQualityProfiles';
export const FETCH_QUALITY_PROFILE_SCHEMA = 'settings/qualityProfiles/fetchQualityProfileSchema';
export const SAVE_QUALITY_PROFILE = 'settings/qualityProfiles/saveQualityProfile';
export const DELETE_QUALITY_PROFILE = 'settings/qualityProfiles/deleteQualityProfile';
export const SET_QUALITY_PROFILE_VALUE = 'settings/qualityProfiles/setQualityProfileValue';

//
// Action Creators

export const fetchQualityProfiles = createThunk(FETCH_QUALITY_PROFILES);
export const fetchQualityProfileSchema = createThunk(FETCH_QUALITY_PROFILE_SCHEMA);
export const saveQualityProfile = createThunk(SAVE_QUALITY_PROFILE);
export const deleteQualityProfile = createThunk(DELETE_QUALITY_PROFILE);

export const setQualityProfileValue = createAction(SET_QUALITY_PROFILE_VALUE, (payload) => {
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
    isDeleting: false,
    deleteError: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: {},
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_QUALITY_PROFILES]: createFetchHandler(section, '/qualityprofile'),
    [FETCH_QUALITY_PROFILE_SCHEMA]: createFetchSchemaHandler(section, '/qualityprofile/schema'),
    [SAVE_QUALITY_PROFILE]: createSaveProviderHandler(section, '/qualityprofile'),
    [DELETE_QUALITY_PROFILE]: createRemoveItemHandler(section, '/qualityprofile')
  },

  //
  // Reducers

  reducers: {
    [SET_QUALITY_PROFILE_VALUE]: createSetSettingValueReducer(section)
  }

};
