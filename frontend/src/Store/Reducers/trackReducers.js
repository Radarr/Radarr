import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createSetClientSideCollectionSortReducer from './Creators/createSetClientSideCollectionSortReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  sortKey: 'mediumNumber',
  sortDirection: sortDirections.DESCENDING,
  items: [],

  columns: [
    {
      name: 'medium',
      label: 'Medium',
      isVisible: true
    },
    {
      name: 'absoluteTrackNumber',
      label: 'Track',
      isVisible: true
    },
    {
      name: 'title',
      label: 'Title',
      isVisible: true
    },
    {
      name: 'duration',
      label: 'Duration',
      isVisible: true
    },
    {
      name: 'audioInfo',
      label: 'Audio Info',
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

export const persistState = [
  'tracks.columns'
];

const reducerSection = 'tracks';

const trackReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),
  [types.UPDATE_ITEM]: createUpdateItemReducer(reducerSection),

  [types.SET_TRACKS_TABLE_OPTION]: createSetTableOptionReducer(reducerSection),

  [types.CLEAR_TRACKS]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  },

  [types.SET_TRACKS_SORT]: createSetClientSideCollectionSortReducer(reducerSection)

}, defaultState);

export default trackReducers;
