import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'dashboard';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  item: {}
};

//
// Actions Types

export const FETCH_DASHBOARD = 'dashboard/fetchDashboard';

//
// Action Creators

export const fetchDashboard = createThunk(FETCH_DASHBOARD);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_DASHBOARD]: createFetchHandler(section, '/dashboard')
});

//
// Reducers

export const reducers = createHandleActions({}, defaultState, section);
