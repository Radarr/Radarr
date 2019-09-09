import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createClearReducer from './Creators/Reducers/createClearReducer';
import albumEntities from 'Album/albumEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { set, removeItem, updateItem } from './baseActions';

//
// Variables

export const section = 'trackFiles';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  sortKey: 'path',
  sortDirection: sortDirections.ASCENDING,

  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: [],

  sortPredicates: {
    quality: function(item, direction) {
      return item.quality ? item.qualityWeight : 0;
    }
  },

  columns: [
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'size',
      label: 'Size',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'dateAdded',
      label: 'Date Added',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'quality',
      label: 'Quality',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'trackFiles.sortKey',
  'trackFiles.sortDirection'
];

//
// Actions Types

export const FETCH_TRACK_FILES = 'trackFiles/fetchTrackFiles';
export const DELETE_TRACK_FILE = 'trackFiles/deleteTrackFile';
export const DELETE_TRACK_FILES = 'trackFiles/deleteTrackFiles';
export const UPDATE_TRACK_FILES = 'trackFiles/updateTrackFiles';
export const SET_TRACK_FILES_SORT = 'trackFiles/setTrackFilesSort';
export const SET_TRACK_FILES_TABLE_OPTION = 'trackFiles/setTrackFilesTableOption';
export const CLEAR_TRACK_FILES = 'trackFiles/clearTrackFiles';

//
// Action Creators

export const fetchTrackFiles = createThunk(FETCH_TRACK_FILES);
export const deleteTrackFile = createThunk(DELETE_TRACK_FILE);
export const deleteTrackFiles = createThunk(DELETE_TRACK_FILES);
export const updateTrackFiles = createThunk(UPDATE_TRACK_FILES);
export const setTrackFilesSort = createAction(SET_TRACK_FILES_SORT);
export const setTrackFilesTableOption = createAction(SET_TRACK_FILES_TABLE_OPTION);
export const clearTrackFiles = createAction(CLEAR_TRACK_FILES);

//
// Helpers

const deleteTrackFileHelper = createRemoveItemHandler(section, '/trackFile');

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_TRACK_FILES]: createFetchHandler(section, '/trackFile'),

  [DELETE_TRACK_FILE]: function(getState, payload, dispatch) {
    const {
      id: trackFileId,
      albumEntity = albumEntities.ALBUMS
    } = payload;

    const albumSection = _.last(albumEntity.split('.'));
    const deletePromise = deleteTrackFileHelper(getState, payload, dispatch);

    deletePromise.done(() => {
      const albums = getState().albums.items;
      const tracksWithRemovedFiles = _.filter(albums, { trackFileId });

      dispatch(batchActions([
        ...tracksWithRemovedFiles.map((track) => {
          return updateItem({
            section: albumSection,
            ...track,
            trackFileId: 0,
            hasFile: false
          });
        })
      ]));
    });
  },

  [DELETE_TRACK_FILES]: function(getState, payload, dispatch) {
    const {
      trackFileIds
    } = payload;

    dispatch(set({ section, isDeleting: true }));

    const promise = createAjaxRequest({
      url: '/trackFile/bulk',
      method: 'DELETE',
      dataType: 'json',
      data: JSON.stringify({ trackFileIds })
    }).request;

    promise.done(() => {
      const tracks = getState().tracks.items;
      const tracksWithRemovedFiles = trackFileIds.reduce((acc, trackFileId) => {
        acc.push(..._.filter(tracks, { trackFileId }));

        return acc;
      }, []);

      dispatch(batchActions([
        ...trackFileIds.map((id) => {
          return removeItem({ section, id });
        }),

        ...tracksWithRemovedFiles.map((track) => {
          return updateItem({
            section: 'tracks',
            ...track,
            trackFileId: 0,
            hasFile: false
          });
        }),

        set({
          section,
          isDeleting: false,
          deleteError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  },

  [UPDATE_TRACK_FILES]: function(getState, payload, dispatch) {
    const {
      trackFileIds,
      quality
    } = payload;

    dispatch(set({ section, isSaving: true }));

    const data = {
      trackFileIds
    };

    if (quality) {
      data.quality = quality;
    }

    const promise = createAjaxRequest({
      url: '/trackFile/editor',
      method: 'PUT',
      dataType: 'json',
      data: JSON.stringify(data)
    }).request;

    promise.done(() => {
      dispatch(batchActions([
        ...trackFileIds.map((id) => {
          const props = {};

          if (quality) {
            props.quality = quality;
          }

          return updateItem({ section, id, ...props });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({
  [SET_TRACK_FILES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_TRACK_FILES_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_TRACK_FILES]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState, section);
