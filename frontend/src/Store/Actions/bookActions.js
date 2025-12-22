import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'books';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {}
};

export const FETCH_BOOKS = 'books/fetchBooks';
export const SET_BOOK_VALUE = 'books/setBookValue';
export const SAVE_BOOK = 'books/saveBook';
export const DELETE_BOOK = 'books/deleteBook';

export const fetchBooks = createThunk(FETCH_BOOKS);
export const saveBook = createThunk(SAVE_BOOK);
export const deleteBook = createThunk(DELETE_BOOK, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles
    }
  };
});

export const setBookValue = createAction(SET_BOOK_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_BOOKS]: createFetchHandler(section, '/book'),
  [SAVE_BOOK]: createSaveProviderHandler(section, '/book'),
  [DELETE_BOOK]: createRemoveItemHandler(section, '/book')
});

export const reducers = createHandleActions({
  [SET_BOOK_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
