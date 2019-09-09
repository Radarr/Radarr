import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'albumHistory';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

//
// Actions Types

export const FETCH_ALBUM_HISTORY = 'albumHistory/fetchAlbumHistory';
export const CLEAR_ALBUM_HISTORY = 'albumHistory/clearAlbumHistory';
export const ALBUM_HISTORY_MARK_AS_FAILED = 'albumHistory/albumHistoryMarkAsFailed';

//
// Action Creators

export const fetchAlbumHistory = createThunk(FETCH_ALBUM_HISTORY);
export const clearAlbumHistory = createAction(CLEAR_ALBUM_HISTORY);
export const albumHistoryMarkAsFailed = createThunk(ALBUM_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_ALBUM_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const queryParams = {
      pageSize: 1000,
      page: 1,
      sortKey: 'date',
      sortDirection: sortDirections.DESCENDING,
      albumId: payload.albumId
    };

    const promise = createAjaxRequest({
      url: '/history',
      data: queryParams
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data: data.records }),

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

  [ALBUM_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      albumId
    } = payload;

    const promise = createAjaxRequest({
      url: '/history/failed',
      method: 'POST',
      data: {
        id: historyId
      }
    }).request;

    promise.done(() => {
      dispatch(fetchAlbumHistory({ albumId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_ALBUM_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

