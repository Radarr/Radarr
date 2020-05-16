import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'authorHistory';

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

export const FETCH_AUTHOR_HISTORY = 'authorHistory/fetchAuthorHistory';
export const CLEAR_AUTHOR_HISTORY = 'authorHistory/clearAuthorHistory';
export const AUTHOR_HISTORY_MARK_AS_FAILED = 'authorHistory/authorHistoryMarkAsFailed';

//
// Action Creators

export const fetchAuthorHistory = createThunk(FETCH_AUTHOR_HISTORY);
export const clearAuthorHistory = createAction(CLEAR_AUTHOR_HISTORY);
export const authorHistoryMarkAsFailed = createThunk(AUTHOR_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_AUTHOR_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/history/author',
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

  [AUTHOR_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      authorId,
      bookId
    } = payload;

    const promise = createAjaxRequest({
      url: '/history/failed',
      method: 'POST',
      data: {
        id: historyId
      }
    }).request;

    promise.done(() => {
      dispatch(fetchAuthorHistory({ authorId, bookId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_AUTHOR_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

