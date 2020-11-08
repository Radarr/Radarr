import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

//
// Variables

export const section = 'blacklist';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  pageSize: 20,
  sortKey: 'date',
  sortDirection: sortDirections.DESCENDING,
  error: null,
  items: [],

  columns: [
    {
      name: 'movies.sortTitle',
      label: translate('MovieTitle'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'sourceTitle',
      label: translate('SourceTitle'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'languages',
      label: translate('Languages'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'quality',
      label: translate('Quality'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'customFormats',
      label: translate('Formats'),
      isSortable: false,
      isVisible: true
    },
    {
      name: 'date',
      label: translate('Date'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'indexer',
      label: translate('Indexer'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'blacklist.pageSize',
  'blacklist.sortKey',
  'blacklist.sortDirection',
  'blacklist.columns'
];

//
// Action Types

export const FETCH_BLACKLIST = 'blacklist/fetchBlacklist';
export const GOTO_FIRST_BLACKLIST_PAGE = 'blacklist/gotoBlacklistFirstPage';
export const GOTO_PREVIOUS_BLACKLIST_PAGE = 'blacklist/gotoBlacklistPreviousPage';
export const GOTO_NEXT_BLACKLIST_PAGE = 'blacklist/gotoBlacklistNextPage';
export const GOTO_LAST_BLACKLIST_PAGE = 'blacklist/gotoBlacklistLastPage';
export const GOTO_BLACKLIST_PAGE = 'blacklist/gotoBlacklistPage';
export const SET_BLACKLIST_SORT = 'blacklist/setBlacklistSort';
export const SET_BLACKLIST_TABLE_OPTION = 'blacklist/setBlacklistTableOption';
export const REMOVE_FROM_BLACKLIST = 'blacklist/removeFromBlacklist';
export const CLEAR_BLACKLIST = 'blacklist/clearBlacklist';

//
// Action Creators

export const fetchBlacklist = createThunk(FETCH_BLACKLIST);
export const gotoBlacklistFirstPage = createThunk(GOTO_FIRST_BLACKLIST_PAGE);
export const gotoBlacklistPreviousPage = createThunk(GOTO_PREVIOUS_BLACKLIST_PAGE);
export const gotoBlacklistNextPage = createThunk(GOTO_NEXT_BLACKLIST_PAGE);
export const gotoBlacklistLastPage = createThunk(GOTO_LAST_BLACKLIST_PAGE);
export const gotoBlacklistPage = createThunk(GOTO_BLACKLIST_PAGE);
export const setBlacklistSort = createThunk(SET_BLACKLIST_SORT);
export const setBlacklistTableOption = createAction(SET_BLACKLIST_TABLE_OPTION);
export const removeFromBlacklist = createThunk(REMOVE_FROM_BLACKLIST);
export const clearBlacklist = createAction(CLEAR_BLACKLIST);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/blacklist',
    fetchBlacklist,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_BLACKLIST,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_BLACKLIST_SORT
    }),

  [REMOVE_FROM_BLACKLIST]: createRemoveItemHandler(section, '/blacklist')
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_BLACKLIST_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_BLACKLIST]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    totalPages: 0,
    totalRecords: 0
  })

}, defaultState, section);
