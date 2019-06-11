import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.customFormats';

//
// Actions Types

export const FETCH_CUSTOM_FORMATS = 'settings/customFormats/fetchCustomFormats';
export const FETCH_CUSTOM_FORMAT_SCHEMA = 'settings/customFormats/fetchCustomFormatSchema';
export const SAVE_CUSTOM_FORMAT = 'settings/customFormats/saveCustomFormat';
export const DELETE_CUSTOM_FORMAT = 'settings/customFormats/deleteCustomFormat';
export const SET_CUSTOM_FORMAT_VALUE = 'settings/customFormats/setCustomFormatValue';
export const CLONE_CUSTOM_FORMAT = 'settings/customFormats/cloneCustomFormat';

//
// Action Creators

export const fetchCustomFormats = createThunk(FETCH_CUSTOM_FORMATS);
export const fetchCustomFormatSchema = createThunk(FETCH_CUSTOM_FORMAT_SCHEMA);
export const saveCustomFormat = createThunk(SAVE_CUSTOM_FORMAT);
export const deleteCustomFormat = createThunk(DELETE_CUSTOM_FORMAT);

export const setCustomFormatValue = createAction(SET_CUSTOM_FORMAT_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneCustomFormat = createAction(CLONE_CUSTOM_FORMAT);

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
    isSchemaFetching: false,
    isSchemaPopulated: false,
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
    [FETCH_CUSTOM_FORMATS]: createFetchHandler(section, '/customformat'),
    [FETCH_CUSTOM_FORMAT_SCHEMA]: createFetchSchemaHandler(section, '/customformat/schema'),
    [SAVE_CUSTOM_FORMAT]: createSaveProviderHandler(section, '/customformat'),
    [DELETE_CUSTOM_FORMAT]: createRemoveItemHandler(section, '/customformat')
  },

  //
  // Reducers

  reducers: {
    [SET_CUSTOM_FORMAT_VALUE]: createSetSettingValueReducer(section),

    [CLONE_CUSTOM_FORMAT]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const item = newState.items.find((i) => i.id === id);
      const pendingChanges = { ...item, id: 0 };
      delete pendingChanges.id;

      pendingChanges.name = `${pendingChanges.name} - Copy`;
      newState.pendingChanges = pendingChanges;

      return updateSectionState(state, section, newState);
    }
  }

};
