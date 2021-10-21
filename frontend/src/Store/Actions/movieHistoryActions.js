import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'movieHistory';

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

export const FETCH_MOVIE_HISTORY = 'movieHistory/fetchMovieHistory';
export const CLEAR_MOVIE_HISTORY = 'movieHistory/clearMovieHistory';
export const MOVIE_HISTORY_MARK_AS_FAILED = 'movieHistory/movieHistoryMarkAsFailed';

//
// Action Creators

export const fetchMovieHistory = createThunk(FETCH_MOVIE_HISTORY);
export const clearMovieHistory = createAction(CLEAR_MOVIE_HISTORY);
export const movieHistoryMarkAsFailed = createThunk(MOVIE_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_MOVIE_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/history/movie',
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

  [MOVIE_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      movieId
    } = payload;

    const promise = createAjaxRequest({
      url: `/history/failed/${historyId}`,
      method: 'POST',
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(fetchMovieHistory({ movieId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_MOVIE_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
