import moment from 'moment';
import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'artistIndex';

//
// State

export const defaultState = {
  sortKey: 'sortName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortName',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true
  },

  bannerOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showMonitored: true,
    showNetwork: true,
    showQualityProfile: true,
    showPreviousAiring: false,
    showAdded: false,
    showAlbumCount: true,
    showPath: false,
    showSizeOnDisk: false
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
      name: 'artistType',
      label: 'Type',
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
      name: 'metadataProfileId',
      label: 'Metadata Profile',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'nextAlbum',
      label: 'Next Album',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'lastAlbum',
      label: 'Last Album',
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
    trackProgress: function(item) {
      const {
        trackCount = 0,
        trackFileCount
      } = item.statistics;

      const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

      return progress + trackCount / 1000000;
    },

    albumCount: function(item) {
      return item.statistics.albumCount;
    },

    trackCount: function(item) {
      return item.statistics.totalTrackCount;
    },

    sizeOnDisk: function(item) {
      return item.statistics.sizeOnDisk;
    }
  },

  selectedFilterKey: 'all',
  // filters come from artistActions
  customFilters: []
  // filterPredicates come from artistActions
};

export const persistState = [
  'artistIndex.sortKey',
  'artistIndex.sortDirection',
  'artistIndex.selectedFilterKey',
  'artistIndex.customFilters',
  'artistIndex.view',
  'artistIndex.columns',
  'artistIndex.posterOptions',
  'artistIndex.bannerOptions',
  'artistIndex.overviewOptions'
];

//
// Actions Types

export const SET_ARTIST_SORT = 'artistIndex/setArtistSort';
export const SET_ARTIST_FILTER = 'artistIndex/setArtistFilter';
export const SET_ARTIST_VIEW = 'artistIndex/setArtistView';
export const SET_ARTIST_TABLE_OPTION = 'artistIndex/setArtistTableOption';
export const SET_ARTIST_POSTER_OPTION = 'artistIndex/setArtistPosterOption';
export const SET_ARTIST_BANNER_OPTION = 'artistIndex/setArtistBannerOption';
export const SET_ARTIST_OVERVIEW_OPTION = 'artistIndex/setArtistOverviewOption';

//
// Action Creators

export const setArtistSort = createAction(SET_ARTIST_SORT);
export const setArtistFilter = createAction(SET_ARTIST_FILTER);
export const setArtistView = createAction(SET_ARTIST_VIEW);
export const setArtistTableOption = createAction(SET_ARTIST_TABLE_OPTION);
export const setArtistPosterOption = createAction(SET_ARTIST_POSTER_OPTION);
export const setArtistBannerOption = createAction(SET_ARTIST_BANNER_OPTION);
export const setArtistOverviewOption = createAction(SET_ARTIST_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_ARTIST_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_ARTIST_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_ARTIST_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_ARTIST_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_ARTIST_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_ARTIST_BANNER_OPTION]: function(state, { payload }) {
    const bannerOptions = state.bannerOptions;

    return {
      ...state,
      bannerOptions: {
        ...bannerOptions,
        ...payload
      }
    };
  },

  [SET_ARTIST_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  }

}, defaultState, section);
