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
  sortKey: 'releaseDate',
  sortDirection: sortDirections.DESCENDING,
  items: [],

  columns: [
    {
      name: 'monitored',
      columnLabel: 'Monitored',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'title',
      label: 'Title',
      isVisible: true
    },
    {
      name: 'path',
      label: 'Path',
      isVisible: false
    },
    {
      name: 'releaseDate',
      label: 'Release Date',
      isVisible: true
    },
    {
      name: 'mediumCount',
      label: 'Media Count',
      isVisible: false
    },
    {
      name: 'trackCount',
      label: 'Track Count',
      isVisible: false
    },
    {
      name: 'duration',
      label: 'Duration',
      isVisible: false
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
  'episodes.columns'
];

const reducerSection = 'episodes';

const episodeReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),
  [types.UPDATE_ITEM]: createUpdateItemReducer(reducerSection),

  [types.SET_EPISODES_TABLE_OPTION]: createSetTableOptionReducer(reducerSection),

  [types.CLEAR_EPISODES]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  },

  [types.SET_EPISODES_SORT]: createSetClientSideCollectionSortReducer(reducerSection)

}, defaultState);

export default episodeReducers;
