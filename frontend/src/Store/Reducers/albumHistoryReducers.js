import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

const reducerSection = 'albumHistory';

const albumHistoryReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),

  [types.CLEAR_ALBUM_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState);

export default albumHistoryReducers;
