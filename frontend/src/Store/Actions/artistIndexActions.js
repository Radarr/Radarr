import { createAction } from 'redux-actions';
import sortByName from 'Utilities/Array/sortByName';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, sortDirections } from 'Helpers/Props';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { filters, filterPredicates, sortPredicates } from './artistActions';

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
    showTitle: true,
    showMonitored: true,
    showQualityProfile: true,
    showSearchAction: false
  },

  bannerOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true,
    showSearchAction: false
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showMonitored: true,
    showQualityProfile: true,
    showLastAlbum: false,
    showAdded: false,
    showAlbumCount: true,
    showPath: false,
    showSizeOnDisk: false,
    showSearchAction: false
  },

  tableOptions: {
    showBanners: false,
    showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: 'Status',
      isSortable: true,
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
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
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
      name: 'genres',
      label: 'Genres',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'ratings',
      label: 'Rating',
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
    ...sortPredicates,

    trackProgress: function(item) {
      const { statistics = {} } = item;

      const {
        trackCount = 0,
        trackFileCount
      } = statistics;

      const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

      return progress + trackCount / 1000000;
    },

    nextAlbum: function(item) {
      if (item.nextAlbum) {
        return item.nextAlbum.releaseDate;
      }
      return '1/1/1000';
    },

    lastAlbum: function(item) {
      if (item.lastAlbum) {
        return item.lastAlbum.releaseDate;
      }
      return '1/1/1000';
    },

    albumCount: function(item) {
      const { statistics = {} } = item;

      return statistics.albumCount;
    },

    trackCount: function(item) {
      const { statistics = {} } = item;

      return statistics.totalTrackCount;
    },

    sizeOnDisk: function(item) {
      const { statistics = {} } = item;

      return statistics.sizeOnDisk;
    },

    ratings: function(item) {
      const { ratings = {} } = item;

      return ratings.value;
    }
  },

  selectedFilterKey: 'all',

  filters,

  filterPredicates: {
    ...filterPredicates,

    trackProgress: function(item, filterValue, type) {
      const { statistics = {} } = item;

      const {
        trackCount = 0,
        trackFileCount
      } = statistics;

      const progress = trackCount ?
        trackFileCount / trackCount * 100 :
        100;

      const predicate = filterTypePredicates[type];

      return predicate(progress, filterValue);
    }
  },

  filterBuilderProps: [
    {
      name: 'monitored',
      label: 'Monitored',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'status',
      label: 'Status',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.ARTIST_STATUS
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'metadataProfileId',
      label: 'Metadata Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.METADATA_PROFILE
    },
    {
      name: 'nextAlbum',
      label: 'Next Album',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'lastAlbum',
      label: 'Last Album',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'added',
      label: 'Added',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'albumCount',
      label: 'Album Count',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'trackProgress',
      label: 'Track Progress',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'path',
      label: 'Path',
      type: filterBuilderTypes.STRING
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.BYTES
    },
    {
      name: 'genres',
      label: 'Genres',
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, artist) => {
          artist.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre
            });
          });

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'ratings',
      label: 'Rating',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'tags',
      label: 'Tags',
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    }
  ]
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
  'artistIndex.overviewOptions',
  'artistIndex.tableOptions'
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
