import $ from 'jquery';
import { createAction } from 'redux-actions';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';

//
// Variables

export const section = 'system';

//
// State

export const defaultState = {
  status: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  health: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  diskSpace: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  tasks: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  backups: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  updates: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  logs: {
    isFetching: false,
    isPopulated: false,
    pageSize: 50,
    sortKey: 'time',
    sortDirection: sortDirections.DESCENDING,
    filterKey: null,
    filterValue: null,
    error: null,
    items: [],

    columns: [
      {
        name: 'level',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'logger',
        label: 'Component',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'message',
        label: 'Message',
        isVisible: true
      },
      {
        name: 'time',
        label: 'Time',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'actions',
        columnLabel: 'Actions',
        isSortable: true,
        isVisible: true,
        isModifiable: false
      }
    ]
  },

  logFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  updateLogFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }
};

export const persistState = [
  'system.logs.pageSize',
  'system.logs.sortKey',
  'system.logs.sortDirection',
  'system.logs.filterKey',
  'system.logs.filterValue'
];

//
// Actions Types

export const FETCH_STATUS = 'system/status/fetchStatus';
export const FETCH_HEALTH = 'system/health/fetchHealth';
export const FETCH_DISK_SPACE = 'system/diskSpace/fetchDiskSPace';

export const FETCH_TASK = 'system/tasks/fetchTask';
export const FETCH_TASKS = 'system/tasks/fetchTasks';
export const FETCH_BACKUPS = 'system/backups/fetchBackups';
export const FETCH_UPDATES = 'system/updates/fetchUpdates';

export const FETCH_LOGS = 'system/logs/fetchLogs';
export const GOTO_FIRST_LOGS_PAGE = 'system/logs/gotoLogsFirstPage';
export const GOTO_PREVIOUS_LOGS_PAGE = 'system/logs/gotoLogsPreviousPage';
export const GOTO_NEXT_LOGS_PAGE = 'system/logs/gotoLogsNextPage';
export const GOTO_LAST_LOGS_PAGE = 'system/logs/gotoLogsLastPage';
export const GOTO_LOGS_PAGE = 'system/logs/gotoLogsPage';
export const SET_LOGS_SORT = 'system/logs/setLogsSort';
export const SET_LOGS_FILTER = 'system/logs/setLogsFilter';
export const SET_LOGS_TABLE_OPTION = 'system/logs/ssetLogsTableOption';

export const FETCH_LOG_FILES = 'system/logFiles/fetchLogFiles';
export const FETCH_UPDATE_LOG_FILES = 'system/updateLogFiles/fetchUpdateLogFiles';

export const RESTART = 'system/restart';
export const SHUTDOWN = 'system/shutdown';

//
// Action Creators

export const fetchStatus = createThunk(FETCH_STATUS);
export const fetchHealth = createThunk(FETCH_HEALTH);
export const fetchDiskSpace = createThunk(FETCH_DISK_SPACE);

export const fetchTask = createThunk(FETCH_TASK);
export const fetchTasks = createThunk(FETCH_TASKS);
export const fetchBackups = createThunk(FETCH_BACKUPS);
export const fetchUpdates = createThunk(FETCH_UPDATES);

export const fetchLogs = createThunk(FETCH_LOGS);
export const gotoLogsFirstPage = createThunk(GOTO_FIRST_LOGS_PAGE);
export const gotoLogsPreviousPage = createThunk(GOTO_PREVIOUS_LOGS_PAGE);
export const gotoLogsNextPage = createThunk(GOTO_NEXT_LOGS_PAGE);
export const gotoLogsLastPage = createThunk(GOTO_LAST_LOGS_PAGE);
export const gotoLogsPage = createThunk(GOTO_LOGS_PAGE);
export const setLogsSort = createThunk(SET_LOGS_SORT);
export const setLogsFilter = createThunk(SET_LOGS_FILTER);
export const setLogsTableOption = createAction(SET_LOGS_TABLE_OPTION);

export const fetchLogFiles = createThunk(FETCH_LOG_FILES);
export const fetchUpdateLogFiles = createThunk(FETCH_UPDATE_LOG_FILES);

export const restart = createThunk(RESTART);
export const shutdown = createThunk(SHUTDOWN);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_STATUS]: createFetchHandler('system.status', '/system/status'),
  [FETCH_HEALTH]: createFetchHandler('system.health', '/health'),
  [FETCH_DISK_SPACE]: createFetchHandler('system.diskSpace', '/diskspace'),
  [FETCH_TASK]: createFetchHandler('system.tasks', '/system/task'),
  [FETCH_TASKS]: createFetchHandler('system.tasks', '/system/task'),
  [FETCH_BACKUPS]: createFetchHandler('system.backups', '/system/backup'),
  [FETCH_UPDATES]: createFetchHandler('system.updates', '/update'),
  [FETCH_LOG_FILES]: createFetchHandler('system.logFiles', '/log/file'),
  [FETCH_UPDATE_LOG_FILES]: createFetchHandler('system.updateLogFiles', '/log/file/update'),

  ...createServerSideCollectionHandlers(
    'system.logs',
    '/log',
    fetchLogs,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_LOGS,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_LOGS_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_LOGS_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_LOGS_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_LOGS_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_LOGS_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_LOGS_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_LOGS_FILTER
    }
  ),

  [RESTART]: function() {
    $.ajax({
      url: '/system/restart',
      method: 'POST'
    });
  },

  [SHUTDOWN]: function() {
    $.ajax({
      url: '/system/shutdown',
      method: 'POST'
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_LOGS_TABLE_OPTION]: createSetTableOptionReducer('logs')

}, defaultState, section);
