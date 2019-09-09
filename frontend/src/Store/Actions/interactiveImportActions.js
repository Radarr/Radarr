import moment from 'moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import { set, update } from './baseActions';

//
// Variables

export const section = 'interactiveImport';

const albumsSection = `${section}.albums`;
const trackFilesSection = `${section}.trackFiles`;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  isSaving: false,
  error: null,
  items: [],
  pendingChanges: {},
  sortKey: 'quality',
  sortDirection: sortDirections.DESCENDING,
  recentFolders: [],
  importMode: 'move',
  sortPredicates: {
    relativePath: function(item, direction) {
      const relativePath = item.relativePath;

      return relativePath.toLowerCase();
    },

    artist: function(item, direction) {
      const artist = item.artist;

      return artist ? artist.sortName : '';
    },

    quality: function(item, direction) {
      return item.quality ? item.qualityWeight : 0;
    }
  },

  albums: {
    isFetching: false,
    isPopulated: false,
    error: null,
    sortKey: 'albumTitle',
    sortDirection: sortDirections.ASCENDING,
    items: []
  },

  trackFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    sortKey: 'relataivePath',
    sortDirection: sortDirections.ASCENDING,
    items: []
  }
};

export const persistState = [
  'interactiveImport.recentFolders',
  'interactiveImport.importMode'
];

//
// Actions Types

export const FETCH_INTERACTIVE_IMPORT_ITEMS = 'interactiveImport/fetchInteractiveImportItems';
export const SAVE_INTERACTIVE_IMPORT_ITEM = 'interactiveImport/saveInteractiveImportItem';
export const SET_INTERACTIVE_IMPORT_SORT = 'interactiveImport/setInteractiveImportSort';
export const UPDATE_INTERACTIVE_IMPORT_ITEM = 'interactiveImport/updateInteractiveImportItem';
export const UPDATE_INTERACTIVE_IMPORT_ITEMS = 'interactiveImport/updateInteractiveImportItems';
export const CLEAR_INTERACTIVE_IMPORT = 'interactiveImport/clearInteractiveImport';
export const ADD_RECENT_FOLDER = 'interactiveImport/addRecentFolder';
export const REMOVE_RECENT_FOLDER = 'interactiveImport/removeRecentFolder';
export const SET_INTERACTIVE_IMPORT_MODE = 'interactiveImport/setInteractiveImportMode';

export const FETCH_INTERACTIVE_IMPORT_ALBUMS = 'interactiveImport/fetchInteractiveImportAlbums';
export const SET_INTERACTIVE_IMPORT_ALBUMS_SORT = 'interactiveImport/clearInteractiveImportAlbumsSort';
export const CLEAR_INTERACTIVE_IMPORT_ALBUMS = 'interactiveImport/clearInteractiveImportAlbums';

export const FETCH_INTERACTIVE_IMPORT_TRACKFILES = 'interactiveImport/fetchInteractiveImportTrackFiles';
export const CLEAR_INTERACTIVE_IMPORT_TRACKFILES = 'interactiveImport/clearInteractiveImportTrackFiles';

//
// Action Creators

export const fetchInteractiveImportItems = createThunk(FETCH_INTERACTIVE_IMPORT_ITEMS);
export const setInteractiveImportSort = createAction(SET_INTERACTIVE_IMPORT_SORT);
export const updateInteractiveImportItem = createAction(UPDATE_INTERACTIVE_IMPORT_ITEM);
export const updateInteractiveImportItems = createAction(UPDATE_INTERACTIVE_IMPORT_ITEMS);
export const saveInteractiveImportItem = createThunk(SAVE_INTERACTIVE_IMPORT_ITEM);
export const clearInteractiveImport = createAction(CLEAR_INTERACTIVE_IMPORT);
export const addRecentFolder = createAction(ADD_RECENT_FOLDER);
export const removeRecentFolder = createAction(REMOVE_RECENT_FOLDER);
export const setInteractiveImportMode = createAction(SET_INTERACTIVE_IMPORT_MODE);

export const fetchInteractiveImportAlbums = createThunk(FETCH_INTERACTIVE_IMPORT_ALBUMS);
export const setInteractiveImportAlbumsSort = createAction(SET_INTERACTIVE_IMPORT_ALBUMS_SORT);
export const clearInteractiveImportAlbums = createAction(CLEAR_INTERACTIVE_IMPORT_ALBUMS);

export const fetchInteractiveImportTrackFiles = createThunk(FETCH_INTERACTIVE_IMPORT_TRACKFILES);
export const clearInteractiveImportTrackFiles = createAction(CLEAR_INTERACTIVE_IMPORT_TRACKFILES);

//
// Action Handlers
export const actionHandlers = handleThunks({
  [FETCH_INTERACTIVE_IMPORT_ITEMS]: function(getState, payload, dispatch) {
    if (!payload.downloadId && !payload.folder) {
      dispatch(set({ section, error: { message: '`downloadId` or `folder` is required.' } }));
      return;
    }

    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/manualimport',
      data: payload
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  },

  [SAVE_INTERACTIVE_IMPORT_ITEM]: createSaveProviderHandler(section, '/manualimport', {}, true),

  [FETCH_INTERACTIVE_IMPORT_ALBUMS]: createFetchHandler(albumsSection, '/album'),

  [FETCH_INTERACTIVE_IMPORT_TRACKFILES]: createFetchHandler(trackFilesSection, '/trackFile')
});

//
// Reducers

export const reducers = createHandleActions({

  [UPDATE_INTERACTIVE_IMPORT_ITEM]: (state, { payload }) => {
    const id = payload.id;
    const newState = Object.assign({}, state);
    const items = newState.items;
    const index = items.findIndex((item) => item.id === id);
    const item = Object.assign({}, items[index], payload);

    newState.items = [...items];
    newState.items.splice(index, 1, item);

    return newState;
  },

  [UPDATE_INTERACTIVE_IMPORT_ITEMS]: (state, { payload }) => {
    const ids = payload.ids;
    const newState = Object.assign({}, state);
    const items = [...newState.items];

    ids.forEach((id) => {
      const index = items.findIndex((item) => item.id === id);
      const item = Object.assign({}, items[index], payload);

      items.splice(index, 1, item);
    });

    newState.items = items;

    return newState;
  },

  [ADD_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolder = { folder, lastUsed: moment().toISOString() };
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    if (index > -1) {
      recentFolders.splice(index, 1, recentFolder);
    } else {
      recentFolders.push(recentFolder);
    }

    return Object.assign({}, state, { recentFolders });
  },

  [REMOVE_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    recentFolders.splice(index, 1);

    return Object.assign({}, state, { recentFolders });
  },

  [CLEAR_INTERACTIVE_IMPORT]: function(state) {
    const newState = {
      ...defaultState,
      recentFolders: state.recentFolders,
      importMode: state.importMode
    };

    return newState;
  },

  [SET_INTERACTIVE_IMPORT_SORT]: createSetClientSideCollectionSortReducer(section),

  [SET_INTERACTIVE_IMPORT_MODE]: function(state, { payload }) {
    return Object.assign({}, state, { importMode: payload.importMode });
  },

  [SET_INTERACTIVE_IMPORT_ALBUMS_SORT]: createSetClientSideCollectionSortReducer(albumsSection),

  [CLEAR_INTERACTIVE_IMPORT_ALBUMS]: (state) => {
    return updateSectionState(state, albumsSection, {
      ...defaultState.albums
    });
  },

  [CLEAR_INTERACTIVE_IMPORT_TRACKFILES]: (state) => {
    return updateSectionState(state, trackFilesSection, {
      ...defaultState.trackFiles
    });
  }

}, defaultState, section);
