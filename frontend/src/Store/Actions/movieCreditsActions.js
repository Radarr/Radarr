import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'movieCredits';

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

export const FETCH_MOVIE_CREDITS = 'movieCredits/fetchMovieCredits';
export const CLEAR_MOVIE_CREDITS = 'movieCredits/clearMovieCredits';

//
// Action Creators

export const fetchMovieCredits = createThunk(FETCH_MOVIE_CREDITS);
export const clearMovieCredits = createAction(CLEAR_MOVIE_CREDITS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_MOVIE_CREDITS]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/credit',
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

  [CLEAR_MOVIE_CREDITS]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
