import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import requestAction from 'Utilities/requestAction';
import updateSectionState from 'Utilities/State/updateSectionState';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'providerOptions';

//
// State

export const defaultState = {
  items: {},
  isFetching: false,
  isPopulated: false,
  error: false
};

//
// Actions Types

export const FETCH_OPTIONS = 'devices/fetchOptions';
export const CLEAR_OPTIONS = 'devices/clearOptions';

//
// Action Creators

export const fetchOptions = createThunk(FETCH_OPTIONS);
export const clearOptions = createAction(CLEAR_OPTIONS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_OPTIONS]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isFetching: true
    }));

    const oldItems = getState().providerOptions.items;
    const itemSection = payload.itemSection;

    const promise = requestAction(payload);

    promise.done((data) => {
      oldItems[itemSection] = data.options || [];

      dispatch(set({
        section,
        isFetching: false,
        isPopulated: true,
        error: null,
        items: oldItems
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_OPTIONS]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState, section);
