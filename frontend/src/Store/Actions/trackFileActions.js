import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
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
  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: []
};

//
// Actions Types

export const FETCH_TRACK_FILES = 'trackFiles/fetchTrackFiles';
export const DELETE_TRACK_FILE = 'trackFiles/deleteTrackFile';
export const DELETE_TRACK_FILES = 'trackFiles/deleteTrackFiles';
export const UPDATE_TRACK_FILES = 'trackFiles/updateTrackFiles';
export const CLEAR_TRACK_FILES = 'trackFiles/clearTrackFiles';

//
// Action Creators

export const fetchTrackFiles = createThunk(FETCH_TRACK_FILES);
export const deleteTrackFile = createThunk(DELETE_TRACK_FILE);
export const deleteTrackFiles = createThunk(DELETE_TRACK_FILES);
export const updateTrackFiles = createThunk(UPDATE_TRACK_FILES);
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

    const promise = $.ajax({
      url: '/trackFile/bulk',
      method: 'DELETE',
      dataType: 'json',
      data: JSON.stringify({ trackFileIds })
    });

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
      language,
      quality
    } = payload;

    dispatch(set({ section, isSaving: true }));

    const data = {
      trackFileIds
    };

    if (language) {
      data.language = language;
    }

    if (quality) {
      data.quality = quality;
    }

    const promise = $.ajax({
      url: '/trackFile/editor',
      method: 'PUT',
      dataType: 'json',
      data: JSON.stringify(data)
    });

    promise.done(() => {
      dispatch(batchActions([
        ...trackFileIds.map((id) => {
          const props = {};

          if (language) {
            props.language = language;
          }

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

  [CLEAR_TRACK_FILES]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
