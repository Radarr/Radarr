import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import bookEntities from 'Book/bookEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createHandleActions from './Creators/createHandleActions';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'books';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  sortKey: 'releaseDate',
  sortDirection: sortDirections.DESCENDING,
  items: [],
  pendingChanges: {},
  sortPredicates: {
    rating: function(item) {
      return item.ratings.value;
    }
  },

  columns: [
    {
      name: 'monitored',
      columnLabel: 'Monitored',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'title',
      label: 'Title',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'releaseDate',
      label: 'Release Date',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'bookCount',
      label: 'Book Count',
      isVisible: false
    },
    {
      name: 'rating',
      label: 'Rating',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'status',
      label: 'Status',
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
  'books.sortKey',
  'books.sortDirection',
  'books.columns'
];

//
// Actions Types

export const FETCH_BOOKS = 'books/fetchBooks';
export const SET_BOOKS_SORT = 'books/setBooksSort';
export const SET_BOOKS_TABLE_OPTION = 'books/setBooksTableOption';
export const CLEAR_BOOKS = 'books/clearBooks';
export const SET_BOOK_VALUE = 'books/setBookValue';
export const SAVE_BOOK = 'books/saveBook';
export const DELETE_BOOK = 'books/deleteBook';
export const TOGGLE_BOOK_MONITORED = 'books/toggleBookMonitored';
export const TOGGLE_BOOKS_MONITORED = 'books/toggleBooksMonitored';

//
// Action Creators

export const fetchBooks = createThunk(FETCH_BOOKS);
export const setBooksSort = createAction(SET_BOOKS_SORT);
export const setBooksTableOption = createAction(SET_BOOKS_TABLE_OPTION);
export const clearBooks = createAction(CLEAR_BOOKS);
export const toggleBookMonitored = createThunk(TOGGLE_BOOK_MONITORED);
export const toggleBooksMonitored = createThunk(TOGGLE_BOOKS_MONITORED);

export const saveBook = createThunk(SAVE_BOOK);

export const deleteBook = createThunk(DELETE_BOOK, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles,
      addImportListExclusion: payload.addImportListExclusion
    }
  };
});

export const setBookValue = createAction(SET_BOOK_VALUE, (payload) => {
  return {
    section: 'books',
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_BOOKS]: createFetchHandler(section, '/book'),
  [SAVE_BOOK]: createSaveProviderHandler(section, '/book'),
  [DELETE_BOOK]: createRemoveItemHandler(section, '/book'),

  [TOGGLE_BOOK_MONITORED]: function(getState, payload, dispatch) {
    const {
      bookId,
      bookEntity = bookEntities.BOOKS,
      monitored
    } = payload;

    const bookSection = _.last(bookEntity.split('.'));

    dispatch(updateItem({
      id: bookId,
      section: bookSection,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/book/${bookId}`,
      method: 'PUT',
      data: JSON.stringify({ monitored }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(updateItem({
        id: bookId,
        section: bookSection,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id: bookId,
        section: bookSection,
        isSaving: false
      }));
    });
  },

  [TOGGLE_BOOKS_MONITORED]: function(getState, payload, dispatch) {
    const {
      bookIds,
      bookEntity = bookEntities.BOOKS,
      monitored
    } = payload;

    dispatch(batchActions(
      bookIds.map((bookId) => {
        return updateItem({
          id: bookId,
          section: bookEntity,
          isSaving: true
        });
      })
    ));

    const promise = createAjaxRequest({
      url: '/book/monitor',
      method: 'PUT',
      data: JSON.stringify({ bookIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions(
        bookIds.map((bookId) => {
          return updateItem({
            id: bookId,
            section: bookEntity,
            isSaving: false,
            monitored
          });
        })
      ));
    });

    promise.fail((xhr) => {
      dispatch(batchActions(
        bookIds.map((bookId) => {
          return updateItem({
            id: bookId,
            section: bookEntity,
            isSaving: false
          });
        })
      ));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_BOOKS_SORT]: createSetClientSideCollectionSortReducer(section),

  [SET_BOOKS_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_BOOK_VALUE]: createSetSettingValueReducer(section),

  [CLEAR_BOOKS]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  }

}, defaultState, section);
