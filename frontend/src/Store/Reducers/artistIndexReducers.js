import moment from 'moment';
import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { filterTypes, sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/createSetClientSideCollectionFilterReducer';

export const defaultState = {
  sortKey: 'sortName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortName',
  secondarySortDirection: sortDirections.ASCENDING,
  filterKey: null,
  filterValue: null,
  filterType: filterTypes.EQUAL,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showQualityProfile: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: 'Status',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortName',
      label: 'Artist Name',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'languageProfileId',
      label: 'Language Profile',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'nextAiring',
      label: 'Next Airing',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'previousAiring',
      label: 'Previous Airing',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: 'Added',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'albumCount',
      label: 'Albums',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'trackProgress',
      label: 'Tracks',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'trackCount',
      label: 'Track Count',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'latestAlbum',
      label: 'Latest Album',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'tags',
      label: 'Tags',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ],

  sortPredicates: {
    nextAiring: function(item, direction) {
      const nextAiring = item.nextAiring;

      if (nextAiring) {
        return moment(nextAiring).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return 0;
      }

      return Number.MAX_VALUE;
    },

    trackProgress: function(item) {
      const {
        trackCount = 0,
        trackFileCount
      } = item;

      const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

      return progress + trackCount / 1000000;
    }
  },

  filterPredicates: {
    missing: function(item) {
      return item.trackCount - item.trackFileCount > 0;
    }
  }
};

export const persistState = [
  'artistIndex.sortKey',
  'artistIndex.sortDirection',
  'artistIndex.filterKey',
  'artistIndex.filterValue',
  'artistIndex.filterType',
  'artistIndex.view',
  'artistIndex.columns',
  'artistIndex.posterOptions'
];

const reducerSection = 'artistIndex';

const artistIndexReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),

  [types.SET_ARTIST_SORT]: createSetClientSideCollectionSortReducer(reducerSection),
  [types.SET_ARTIST_FILTER]: createSetClientSideCollectionFilterReducer(reducerSection),

  [types.SET_ARTIST_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [types.SET_ARTIST_TABLE_OPTION]: createSetTableOptionReducer(reducerSection),

  [types.SET_ARTIST_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  }

}, defaultState);

export default artistIndexReducers;
