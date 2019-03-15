import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'retagPreview';

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

export const FETCH_RETAG_PREVIEW = 'retagPreview/fetchRetagPreview';
export const CLEAR_RETAG_PREVIEW = 'retagPreview/clearRetagPreview';

//
// Action Creators

export const fetchRetagPreview = createThunk(FETCH_RETAG_PREVIEW);
export const clearRetagPreview = createAction(CLEAR_RETAG_PREVIEW);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_RETAG_PREVIEW]: createFetchHandler('retagPreview', '/retag')

});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_RETAG_PREVIEW]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
