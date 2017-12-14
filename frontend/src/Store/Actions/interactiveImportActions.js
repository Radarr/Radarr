import _ from 'lodash';
import $ from 'jquery';
import moment from 'moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'interactiveImport';

const albumsSection = `${section}.albums`;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
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
      return item.quality.qualityWeight;
    }
  },

  albums: {
    isFetching: false,
    isPopulated: false,
    error: null,
    sortKey: 'albumTitle',
    sortDirection: sortDirections.DESCENDING,
    items: []
  }
};

export const persistState = [
  'interactiveImport.recentFolders',
  'interactiveImport.importMode'
];

//
// Actions Types

export const FETCH_INTERACTIVE_IMPORT_ITEMS = 'FETCH_INTERACTIVE_IMPORT_ITEMS';
export const UPDATE_INTERACTIVE_IMPORT_ITEM = 'UPDATE_INTERACTIVE_IMPORT_ITEM';
export const SET_INTERACTIVE_IMPORT_SORT = 'SET_INTERACTIVE_IMPORT_SORT';
export const CLEAR_INTERACTIVE_IMPORT = 'CLEAR_INTERACTIVE_IMPORT';
export const ADD_RECENT_FOLDER = 'ADD_RECENT_FOLDER';
export const REMOVE_RECENT_FOLDER = 'REMOVE_RECENT_FOLDER';
export const SET_INTERACTIVE_IMPORT_MODE = 'SET_INTERACTIVE_IMPORT_MODE';

export const FETCH_INTERACTIVE_IMPORT_ALBUMS = 'FETCH_INTERACTIVE_IMPORT_ALBUMS';
export const SET_INTERACTIVE_IMPORT_ALBUMS_SORT = 'SET_INTERACTIVE_IMPORT_ALBUMS_SORT';
export const CLEAR_INTERACTIVE_IMPORT_ALBUMS = 'CLEAR_INTERACTIVE_IMPORT_ALBUMS';

//
// Action Creators

export const fetchInteractiveImportItems = createThunk(FETCH_INTERACTIVE_IMPORT_ITEMS);
export const setInteractiveImportSort = createAction(SET_INTERACTIVE_IMPORT_SORT);
export const updateInteractiveImportItem = createAction(UPDATE_INTERACTIVE_IMPORT_ITEM);
export const clearInteractiveImport = createAction(CLEAR_INTERACTIVE_IMPORT);
export const addRecentFolder = createAction(ADD_RECENT_FOLDER);
export const removeRecentFolder = createAction(REMOVE_RECENT_FOLDER);
export const setInteractiveImportMode = createAction(SET_INTERACTIVE_IMPORT_MODE);

export const fetchInteractiveImportAlbums = createThunk(FETCH_INTERACTIVE_IMPORT_ALBUMS);
export const setInteractiveImportAlbumsSort = createAction(SET_INTERACTIVE_IMPORT_ALBUMS_SORT);
export const clearInteractiveImportAlbums = createAction(CLEAR_INTERACTIVE_IMPORT_ALBUMS);

//
// Action Handlers
export const actionHandlers = handleThunks({
  [FETCH_INTERACTIVE_IMPORT_ITEMS]: function(getState, payload, dispatch) {
    if (!payload.downloadId && !payload.folder) {
      dispatch(set({ section, error: { message: '`downloadId` or `folder` is required.' } }));
      return;
    }

    dispatch(set({ section, isFetching: true }));

    const promise = $.ajax({
      url: '/manualimport',
      data: payload
    });

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

  [FETCH_INTERACTIVE_IMPORT_ALBUMS]: createFetchHandler('interactiveImport.albums', '/album')
});

//
// Reducers

export const reducers = createHandleActions({

  [UPDATE_INTERACTIVE_IMPORT_ITEM]: (state, { payload }) => {
    const id = payload.id;
    const newState = Object.assign({}, state);
    const items = newState.items;
    const index = _.findIndex(items, { id });
    const item = Object.assign({}, items[index], payload);

    newState.items = [...items];
    newState.items.splice(index, 1, item);

    return newState;
  },

  [ADD_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolder = { folder, lastUsed: moment().toISOString() };
    const recentFolders = [...state.recentFolders];
    const index = _.findIndex(recentFolders, { folder });

    if (index > -1) {
      recentFolders.splice(index, 1, recentFolder);
    } else {
      recentFolders.push(recentFolder);
    }

    return Object.assign({}, state, { recentFolders });
  },

  [REMOVE_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolders = _.remove([...state.recentFolders], { folder });

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
  }

}, defaultState, section);
