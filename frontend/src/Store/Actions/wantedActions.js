import { createAction } from 'redux-actions';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createBatchToggleEpisodeMonitoredHandler from './Creators/createBatchToggleEpisodeMonitoredHandler';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

//
// Variables

export const section = 'wanted';

//
// State

export const defaultState = {
  missing: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: undefined,
    sortDirection: sortDirections.DESCENDING,
    error: null,
    items: [],

    columns: [
      {
        name: 'sortTitle',
        label: 'Title',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'inCinemas',
        label: 'Release Date',
        isSortable: true,
        isVisible: true
      }
    ],

    selectedFilterKey: 'monitored',

    filters: [
      {
        key: 'monitored',
        label: 'Monitored',
        filters: [
          {
            key: 'monitored',
            value: true,
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'unmonitored',
        label: 'Unmonitored',
        filters: [
          {
            key: 'monitored',
            value: false,
            type: filterTypes.EQUAL
          }
        ]
      }
    ]
  }
};

export const persistState = [
  'wanted.missing.pageSize',
  'wanted.missing.sortKey',
  'wanted.missing.sortDirection',
  'wanted.missing.selectedFilterKey',
  'wanted.missing.columns'
];

//
// Actions Types

export const FETCH_MISSING = 'wanted/missing/fetchMissing';
export const GOTO_FIRST_MISSING_PAGE = 'wanted/missing/gotoMissingFirstPage';
export const GOTO_PREVIOUS_MISSING_PAGE = 'wanted/missing/gotoMissingPreviousPage';
export const GOTO_NEXT_MISSING_PAGE = 'wanted/missing/gotoMissingNextPage';
export const GOTO_LAST_MISSING_PAGE = 'wanted/missing/gotoMissingLastPage';
export const GOTO_MISSING_PAGE = 'wanted/missing/gotoMissingPage';
export const SET_MISSING_SORT = 'wanted/missing/setMissingSort';
export const SET_MISSING_FILTER = 'wanted/missing/setMissingFilter';
export const SET_MISSING_TABLE_OPTION = 'wanted/missing/setMissingTableOption';
export const CLEAR_MISSING = 'wanted/missing/clearMissing';

export const BATCH_TOGGLE_MISSING_MOVIES = 'wanted/missing/batchToggleMissingMovies';

//
// Action Creators

export const fetchMissing = createThunk(FETCH_MISSING);
export const gotoMissingFirstPage = createThunk(GOTO_FIRST_MISSING_PAGE);
export const gotoMissingPreviousPage = createThunk(GOTO_PREVIOUS_MISSING_PAGE);
export const gotoMissingNextPage = createThunk(GOTO_NEXT_MISSING_PAGE);
export const gotoMissingLastPage = createThunk(GOTO_LAST_MISSING_PAGE);
export const gotoMissingPage = createThunk(GOTO_MISSING_PAGE);
export const setMissingSort = createThunk(SET_MISSING_SORT);
export const setMissingFilter = createThunk(SET_MISSING_FILTER);
export const setMissingTableOption = createAction(SET_MISSING_TABLE_OPTION);
export const clearMissing = createAction(CLEAR_MISSING);

export const batchToggleMissingMovies = createThunk(BATCH_TOGGLE_MISSING_MOVIES);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    'wanted.missing',
    '/wanted/missing',
    fetchMissing,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_MISSING,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_MISSING_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_MISSING_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_MISSING_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_MISSING_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_MISSING_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_MISSING_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_MISSING_FILTER
    }
  ),

  [BATCH_TOGGLE_MISSING_MOVIES]: createBatchToggleEpisodeMonitoredHandler('wanted.missing', fetchMissing)

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_MISSING_TABLE_OPTION]: createSetTableOptionReducer('wanted.missing'),

  [CLEAR_MISSING]: createClearReducer(
    'wanted.missing',
    {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: [],
      totalPages: 0,
      totalRecords: 0
    }
  )

}, defaultState, section);
