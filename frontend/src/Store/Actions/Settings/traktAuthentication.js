import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';

//
// Variables

const section = 'settings.traktAuthentication';

//
// Actions Types

export const FETCH_TRAKT_AUTHENTICATION = 'settings/traktAuthentication/fetchTraktAuthentication';
export const SAVE_TRAKT_AUTHENTICATION = 'settings/traktAuthentication/saveTraktAuthentication';
export const SET_TRAKT_AUTHENTICATION_VALUE = 'settings/traktAuthentication/setTraktAuthenticationValue';

//
// Action Creators

export const fetchTraktAuthentication = createThunk(FETCH_TRAKT_AUTHENTICATION);
export const saveTraktAuthentication = createThunk(SAVE_TRAKT_AUTHENTICATION);
export const setTraktAuthenticationValue = createAction(SET_TRAKT_AUTHENTICATION_VALUE, (payload) => {
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
    [FETCH_TRAKT_AUTHENTICATION]: createFetchHandler(section, '/config/traktauthentication'),
    [SAVE_TRAKT_AUTHENTICATION]: createSaveHandler(section, '/config/traktauthentication')
  },

  //
  // Reducers

  reducers: {
    [SET_TRAKT_AUTHENTICATION_VALUE]: createSetSettingValueReducer(section)
  }

};
