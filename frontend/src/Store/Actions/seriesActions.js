import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

//
// Variables

export const section = 'series';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  sortKey: 'position',
  sortDirection: sortDirections.ASCENDING,
  items: [],

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
      name: 'position',
      label: 'Number',
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
      name: 'secondaryTypes',
      label: 'Secondary Types',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'mediumCount',
      label: 'Media Count',
      isVisible: false
    },
    {
      name: 'bookCount',
      label: 'Book Count',
      isVisible: false
    },
    {
      name: 'duration',
      label: 'Duration',
      isSortable: true,
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

//
// Actions Types

export const FETCH_SERIES = 'series/fetchSeries';
export const SET_SERIES_SORT = 'books/setSeriesSort';
export const CLEAR_SERIES = 'series/clearSeries';
export const SET_SERIES_VALUE = 'books/setBookValue';

//
// Action Creators

export const fetchSeries = createThunk(FETCH_SERIES);
export const setSeriesSort = createAction(SET_SERIES_SORT);
export const clearSeries = createAction(CLEAR_SERIES);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_SERIES]: createFetchHandler(section, '/series')
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_SORT]: createSetClientSideCollectionSortReducer(section),

  [SET_SERIES_VALUE]: createSetSettingValueReducer(section),

  [CLEAR_SERIES]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  }

}, defaultState, section);
