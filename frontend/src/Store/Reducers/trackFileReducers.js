import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createRemoveItemReducer from './Creators/createRemoveItemReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: []
};

const reducerSection = 'trackFiles';

const trackFileReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),
  [types.UPDATE_ITEM]: createUpdateItemReducer(reducerSection),
  [types.REMOVE_ITEM]: createRemoveItemReducer(reducerSection),

  [types.CLEAR_TRACK_FILES]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState);

export default trackFileReducers;
