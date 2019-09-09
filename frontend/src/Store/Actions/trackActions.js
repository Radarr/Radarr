import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'tracks';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  sortKey: 'mediumNumber',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'absoluteTrackNumber',
  secondarySortDirection: sortDirections.ASCENDING,
  items: [],

  columns: [
    {
      name: 'medium',
      label: 'Medium',
      isVisible: false
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
      name: 'path',
      label: 'Path',
      isVisible: false
    },
    {
      name: 'relativePath',
      label: 'Relative Path',
      isVisible: false
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
  'tracks.sortKey',
  'tracks.sortDirection',
  'tracks.columns'
];

//
// Actions Types

export const FETCH_TRACKS = 'tracks/fetchTracks';
export const SET_TRACKS_SORT = 'tracks/setTracksSort';
export const SET_TRACKS_TABLE_OPTION = 'tracks/setTracksTableOption';
export const CLEAR_TRACKS = 'tracks/clearTracks';

//
// Action Creators

export const fetchTracks = createThunk(FETCH_TRACKS);
export const setTracksSort = createAction(SET_TRACKS_SORT);
export const setTracksTableOption = createAction(SET_TRACKS_TABLE_OPTION);
export const clearTracks = createAction(CLEAR_TRACKS);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_TRACKS]: createFetchHandler(section, '/track')

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_TRACKS_TABLE_OPTION]: createSetTableOptionReducer(section),

  [FETCH_TRACKS]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  },

  [SET_TRACKS_SORT]: createSetClientSideCollectionSortReducer(section)

}, defaultState, section);
