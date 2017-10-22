import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { filterTypes, sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createSetClientSideCollectionSortReducer from './Creators/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/createSetClientSideCollectionFilterReducer';

export const defaultState = {
  isSaving: false,
  saveError: null,
  sortKey: 'sortName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortName',
  secondarySortDirection: sortDirections.ASCENDING,
  filterKey: null,
  filterValue: null,
  filterType: filterTypes.EQUAL
};

export const persistState = [
  'albumStudio.sortKey',
  'albumStudio.sortDirection',
  'albumStudio.filterKey',
  'albumStudio.filterValue',
  'albumStudio.filterType'
];

const reducerSection = 'albumStudio';

const albumStudioReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),

  [types.SET_ALBUM_STUDIO_SORT]: createSetClientSideCollectionSortReducer(reducerSection),
  [types.SET_ALBUM_STUDIO_FILTER]: createSetClientSideCollectionFilterReducer(reducerSection)

}, defaultState);

export default albumStudioReducers;
