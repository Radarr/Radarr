import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.plex';

//
// Actions Types

export const FETCH_PLEX_RESOURCES = 'settings/plex/fetchResources';

//
// Action Creators

export const fetchPlexResources = createThunk(FETCH_PLEX_RESOURCES);

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
    items: []
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_PLEX_RESOURCES]: createFetchHandler(section, '/authentication/plex/resources')
  },

  //
  // Reducers

  reducers: { }
};
