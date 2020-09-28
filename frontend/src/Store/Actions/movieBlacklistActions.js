import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'movieBlacklist';

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

export const FETCH_MOVIE_BLACKLIST = 'movieBlacklist/fetchMovieBlacklist';
export const CLEAR_MOVIE_BLACKLIST = 'movieBlacklist/clearMovieBlacklist';

//
// Action Creators

export const fetchMovieBlacklist = createThunk(FETCH_MOVIE_BLACKLIST);
export const clearMovieBlacklist = createAction(CLEAR_MOVIE_BLACKLIST);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_MOVIE_BLACKLIST]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/blacklist/movie',
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
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_MOVIE_BLACKLIST]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

