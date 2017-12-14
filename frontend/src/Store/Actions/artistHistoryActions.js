import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'artistHistory';

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

export const FETCH_ARTIST_HISTORY = 'artistHistory/fetchArtistHistory';
export const CLEAR_ARTIST_HISTORY = 'artistHistory/clearArtistHistory';
export const ARTIST_HISTORY_MARK_AS_FAILED = 'artistHistory/artistHistoryMarkAsFailed';

//
// Action Creators

export const fetchArtistHistory = createThunk(FETCH_ARTIST_HISTORY);
export const clearArtistHistory = createAction(CLEAR_ARTIST_HISTORY);
export const artistHistoryMarkAsFailed = createThunk(ARTIST_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_ARTIST_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = $.ajax({
      url: '/history/artist',
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

  [ARTIST_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      artistId,
      albumId
    } = payload;

    const promise = $.ajax({
      url: '/history/failed',
      method: 'POST',
      data: {
        id: historyId
      }
    });

    promise.done(() => {
      dispatch(fetchArtistHistory({ artistId, albumId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_ARTIST_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

