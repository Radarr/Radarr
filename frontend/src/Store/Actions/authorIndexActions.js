import { createAction } from 'redux-actions';
import sortByName from 'Utilities/Array/sortByName';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, sortDirections } from 'Helpers/Props';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { filters, filterPredicates, sortPredicates } from './authorActions';

//
// Variables

export const section = 'authorIndex';

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
    showLastBook: false,
    showAdded: false,
    showBookCount: true,
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
      label: 'Author Name',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'authorType',
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
      name: 'nextBook',
      label: 'Next Book',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'lastBook',
      label: 'Last Book',
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
      name: 'bookProgress',
      label: 'Books',
      isSortable: true,
      isVisible: true
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

    bookProgress: function(item) {
      const { statistics = {} } = item;

      const {
        bookCount = 0,
        bookFileCount
      } = statistics;

      const progress = bookCount ? bookFileCount / bookCount * 100 : 100;

      return progress + bookCount / 1000000;
    },

    nextBook: function(item) {
      if (item.nextBook) {
        return item.nextBook.releaseDate;
      }
      return '1/1/1000';
    },

    lastBook: function(item) {
      if (item.lastBook) {
        return item.lastBook.releaseDate;
      }
      return '1/1/1000';
    },

    bookCount: function(item) {
      const { statistics = {} } = item;

      return statistics.bookCount;
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

    bookProgress: function(item, filterValue, type) {
      const { statistics = {} } = item;

      const {
        bookCount = 0,
        bookFileCount
      } = statistics;

      const progress = bookCount ?
        bookFileCount / bookCount * 100 :
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
      valueType: filterBuilderValueTypes.AUTHOR_STATUS
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
      name: 'nextBook',
      label: 'Next Book',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'lastBook',
      label: 'Last Book',
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
      name: 'bookCount',
      label: 'Book Count',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'bookProgress',
      label: 'Book Progress',
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
        const tagList = items.reduce((acc, author) => {
          author.genres.forEach((genre) => {
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
  'authorIndex.sortKey',
  'authorIndex.sortDirection',
  'authorIndex.selectedFilterKey',
  'authorIndex.customFilters',
  'authorIndex.view',
  'authorIndex.columns',
  'authorIndex.posterOptions',
  'authorIndex.bannerOptions',
  'authorIndex.overviewOptions',
  'authorIndex.tableOptions'
];

//
// Actions Types

export const SET_AUTHOR_SORT = 'authorIndex/setAuthorSort';
export const SET_AUTHOR_FILTER = 'authorIndex/setAuthorFilter';
export const SET_AUTHOR_VIEW = 'authorIndex/setAuthorView';
export const SET_AUTHOR_TABLE_OPTION = 'authorIndex/setAuthorTableOption';
export const SET_AUTHOR_POSTER_OPTION = 'authorIndex/setAuthorPosterOption';
export const SET_AUTHOR_BANNER_OPTION = 'authorIndex/setAuthorBannerOption';
export const SET_AUTHOR_OVERVIEW_OPTION = 'authorIndex/setAuthorOverviewOption';

//
// Action Creators

export const setAuthorSort = createAction(SET_AUTHOR_SORT);
export const setAuthorFilter = createAction(SET_AUTHOR_FILTER);
export const setAuthorView = createAction(SET_AUTHOR_VIEW);
export const setAuthorTableOption = createAction(SET_AUTHOR_TABLE_OPTION);
export const setAuthorPosterOption = createAction(SET_AUTHOR_POSTER_OPTION);
export const setAuthorBannerOption = createAction(SET_AUTHOR_BANNER_OPTION);
export const setAuthorOverviewOption = createAction(SET_AUTHOR_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_AUTHOR_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_AUTHOR_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_AUTHOR_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_AUTHOR_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_AUTHOR_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_AUTHOR_BANNER_OPTION]: function(state, { payload }) {
    const bannerOptions = state.bannerOptions;

    return {
      ...state,
      bannerOptions: {
        ...bannerOptions,
        ...payload
      }
    };
  },

  [SET_AUTHOR_OVERVIEW_OPTION]: function(state, { payload }) {
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
