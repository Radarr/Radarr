import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'bookHistory';

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

export const FETCH_BOOK_HISTORY = 'bookHistory/fetchBookHistory';
export const CLEAR_BOOK_HISTORY = 'bookHistory/clearBookHistory';
export const BOOK_HISTORY_MARK_AS_FAILED = 'bookHistory/bookHistoryMarkAsFailed';

//
// Action Creators

export const fetchBookHistory = createThunk(FETCH_BOOK_HISTORY);
export const clearBookHistory = createAction(CLEAR_BOOK_HISTORY);
export const bookHistoryMarkAsFailed = createThunk(BOOK_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_BOOK_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const queryParams = {
      pageSize: 1000,
      page: 1,
      sortKey: 'date',
      sortDirection: sortDirections.DESCENDING,
      bookId: payload.bookId
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

  [BOOK_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
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
      dispatch(fetchBookHistory({ bookId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_BOOK_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

