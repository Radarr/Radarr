import $ from 'jquery';
import { createAction } from 'redux-actions';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'history';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  pageSize: 20,
  sortKey: 'date',
  sortDirection: sortDirections.DESCENDING,
  filterKey: null,
  filterValue: null,
  items: [],

  columns: [
    {
      name: 'eventType',
      columnLabel: 'Event Type',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'artist.sortName',
      label: 'Artist',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'albumTitle',
      label: 'Album Title',
      isVisible: true
    },
    {
      name: 'trackTitle',
      label: 'Track Title',
      isVisible: true
    },
    {
      name: 'language',
      label: 'Language',
      isVisible: false
    },
    {
      name: 'quality',
      label: 'Quality',
      isVisible: true
    },
    {
      name: 'date',
      label: 'Date',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'downloadClient',
      label: 'Download Client',
      isVisible: false
    },
    {
      name: 'indexer',
      label: 'Indexer',
      isVisible: false
    },
    {
      name: 'releaseGroup',
      label: 'Release Group',
      isVisible: false
    },
    {
      name: 'details',
      columnLabel: 'Details',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'history.pageSize',
  'history.sortKey',
  'history.sortDirection',
  'history.filterKey',
  'history.filterValue'
];

//
// Actions Types

export const FETCH_HISTORY = 'history/fetchHistory';
export const GOTO_FIRST_HISTORY_PAGE = 'history/gotoHistoryFirstPage';
export const GOTO_PREVIOUS_HISTORY_PAGE = 'history/gotoHistoryPreviousPage';
export const GOTO_NEXT_HISTORY_PAGE = 'history/gotoHistoryNextPage';
export const GOTO_LAST_HISTORY_PAGE = 'history/gotoHistoryLastPage';
export const GOTO_HISTORY_PAGE = 'history/gotoHistoryPage';
export const SET_HISTORY_SORT = 'history/setHistorySort';
export const SET_HISTORY_FILTER = 'history/setHistoryFilter';
export const SET_HISTORY_TABLE_OPTION = 'history/setHistoryTableOption';
export const CLEAR_HISTORY = 'history/clearHistory';
export const MARK_AS_FAILED = 'history/markAsFailed';

//
// Action Creators

export const fetchHistory = createThunk(FETCH_HISTORY);
export const gotoHistoryFirstPage = createThunk(GOTO_FIRST_HISTORY_PAGE);
export const gotoHistoryPreviousPage = createThunk(GOTO_PREVIOUS_HISTORY_PAGE);
export const gotoHistoryNextPage = createThunk(GOTO_NEXT_HISTORY_PAGE);
export const gotoHistoryLastPage = createThunk(GOTO_LAST_HISTORY_PAGE);
export const gotoHistoryPage = createThunk(GOTO_HISTORY_PAGE);
export const setHistorySort = createThunk(SET_HISTORY_SORT);
export const setHistoryFilter = createThunk(SET_HISTORY_FILTER);
export const setHistoryTableOption = createAction(SET_HISTORY_TABLE_OPTION);
export const clearHistory = createAction(CLEAR_HISTORY);
export const markAsFailed = createThunk(MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/history',
    fetchHistory,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_HISTORY,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_HISTORY_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_HISTORY_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_HISTORY_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_HISTORY_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_HISTORY_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_HISTORY_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_HISTORY_FILTER
    }),

  [MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const id = payload.id;

    dispatch(updateItem({
      section,
      id,
      isMarkingAsFailed: true
    }));

    const promise = $.ajax({
      url: '/history/failed',
      method: 'POST',
      data: {
        id
      }
    });

    promise.done(() => {
      dispatch(updateItem({
        section,
        id,
        isMarkingAsFailed: false,
        markAsFailedError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        section,
        id,
        isMarkingAsFailed: false,
        markAsFailedError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_HISTORY_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_HISTORY]: createClearReducer('history', {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState, section);
