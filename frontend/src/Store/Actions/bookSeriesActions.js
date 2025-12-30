import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'bookSeries';

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

export const FETCH_BOOK_SERIES = 'bookSeries/fetchBookSeries';
export const SET_BOOK_SERIES_VALUE = 'bookSeries/setBookSeriesValue';
export const SAVE_BOOK_SERIES = 'bookSeries/saveBookSeries';
export const DELETE_BOOK_SERIES = 'bookSeries/deleteBookSeries';

export const fetchBookSeries = createThunk(FETCH_BOOK_SERIES);
export const saveBookSeries = createThunk(SAVE_BOOK_SERIES);
export const deleteBookSeries = createThunk(DELETE_BOOK_SERIES);

export const setBookSeriesValue = createAction(SET_BOOK_SERIES_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_BOOK_SERIES]: createFetchHandler(section, '/bookseries'),
  [SAVE_BOOK_SERIES]: createSaveProviderHandler(section, '/bookseries'),
  [DELETE_BOOK_SERIES]: createRemoveItemHandler(section, '/bookseries')
});

export const reducers = createHandleActions({
  [SET_BOOK_SERIES_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
