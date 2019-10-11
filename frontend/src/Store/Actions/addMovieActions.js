import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import createFetchHandler from './Creators/createFetchHandler';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import sortByName from 'Utilities/Array/sortByName';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, update, updateItem } from './baseActions';
import { filterPredicates, sortPredicates } from './movieActions';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createClearReducer from './Creators/Reducers/createClearReducer';

//
// Variables

export const section = 'addMovie';
let abortCurrentRequest = null;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isAdding: false,
  isAdded: false,
  addError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'overview',

  defaults: {
    rootFolderPath: '',
    monitor: 'true',
    qualityProfileId: 0,
    minimumAvailability: 'announced',
    tags: []
  },

  posterOptions: {
    size: 'large',
    showTitle: false
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showStudio: true
  },

  tableOptions: {
    // showSearchAction: false
  },

  columns: [
    {
      name: 'select',
      columnLabel: 'select',
      isSortable: false,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'status',
      columnLabel: 'Status',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: 'Movie Title',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'studio',
      label: 'Studio',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'inCinemas',
      label: 'In Cinemas',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'physicalRelease',
      label: 'Physical Release',
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
      name: 'certification',
      label: 'Certification',
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

    studio: function(item) {
      const studio = item.studio;

      return studio ? studio.toLowerCase() : '';
    },

    ratings: function(item) {
      const { ratings = {} } = item;

      return ratings.value;
    }
  },

  selectedFilterKey: 'all',

  filters: [
    {
      key: 'all',
      label: 'All',
      filters: []
    }
  ],

  filterPredicates,

  filterBuilderProps: [
    {
      name: 'status',
      label: 'Status',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.SERIES_STATUS
    },
    {
      name: 'studio',
      label: 'Studio',
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, movie) => {
          acc.push({
            id: movie.studio,
            name: movie.studio
          });

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'inCinemas',
      label: 'In Cinemas',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'physicalRelease',
      label: 'Physical Release',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'genres',
      label: 'Genres',
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, movie) => {
          movie.genres.forEach((genre) => {
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
      name: 'certification',
      label: 'Certification',
      type: filterBuilderTypes.EXACT
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
  'addMovie.defaults',
  'addMovie.sortKey',
  'addMovie.sortDirection',
  'addMovie.selectedFilterKey',
  'addMovie.customFilters',
  'addMovie.view',
  'addMovie.columns',
  'addMovie.posterOptions',
  'addMovie.overviewOptions',
  'addMovie.tableOptions'
];

//
// Actions Types

export const LOOKUP_MOVIE = 'addMovie/lookupMovie';
export const ADD_MOVIE = 'addMovie/addMovie';
export const SET_ADD_MOVIE_VALUE = 'addMovie/setAddMovieValue';
export const CLEAR_ADD_MOVIE = 'addMovie/clearAddMovie';
export const SET_ADD_MOVIE_DEFAULT = 'addMovie/setAddMovieDefault';

export const FETCH_LIST_MOVIES = 'addMovie/fetchListMovies';
export const FETCH_DISCOVER_MOVIES = 'addMovie/fetchDiscoverMovies';

export const SET_LIST_MOVIE_SORT = 'addMovie/setListMovieSort';
export const SET_LIST_MOVIE_FILTER = 'addMovie/setListMovieFilter';
export const SET_LIST_MOVIE_VIEW = 'addMovie/setListMovieView';
export const SET_LIST_MOVIE_TABLE_OPTION = 'addMovie/setListMovieTableOption';
export const SET_LIST_MOVIE_POSTER_OPTION = 'addMovie/setListMoviePosterOption';
export const SET_LIST_MOVIE_OVERVIEW_OPTION = 'addMovie/setListMovieOverviewOption';

//
// Action Creators

export const lookupMovie = createThunk(LOOKUP_MOVIE);
export const addMovie = createThunk(ADD_MOVIE);
export const clearAddMovie = createAction(CLEAR_ADD_MOVIE);
export const setAddMovieDefault = createAction(SET_ADD_MOVIE_DEFAULT);

export const fetchListMovies = createThunk(FETCH_LIST_MOVIES);
export const fetchDiscoverMovies = createThunk(FETCH_DISCOVER_MOVIES);

export const setListMovieSort = createAction(SET_LIST_MOVIE_SORT);
export const setListMovieFilter = createAction(SET_LIST_MOVIE_FILTER);
export const setListMovieView = createAction(SET_LIST_MOVIE_VIEW);
export const setListMovieTableOption = createAction(SET_LIST_MOVIE_TABLE_OPTION);
export const setListMoviePosterOption = createAction(SET_LIST_MOVIE_POSTER_OPTION);
export const setListMovieOverviewOption = createAction(SET_LIST_MOVIE_OVERVIEW_OPTION);

export const setAddMovieValue = createAction(SET_ADD_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_LIST_MOVIES]: createFetchHandler(section, '/netimport/movies'),

  [FETCH_DISCOVER_MOVIES]: createFetchHandler(section, '/movies/discover'),

  [LOOKUP_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/movie/lookup',
      data: {
        term: payload.term
      }
    });

    abortCurrentRequest = abortRequest;

    request.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });
  },

  [ADD_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const tmdbId = payload.tmdbId;
    const items = getState().addMovie.items;
    const newMovie = getNewMovie(_.cloneDeep(_.find(items, { tmdbId })), payload);

    const promise = createAjaxRequest({
      url: '/movie',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(newMovie)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'movies', ...data }),

        set({
          section,
          isAdding: false,
          isAdded: true,
          addError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isAdding: false,
        isAdded: false,
        addError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_ADD_MOVIE_VALUE]: createSetSettingValueReducer(section),

  [SET_ADD_MOVIE_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [SET_LIST_MOVIE_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_LIST_MOVIE_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_LIST_MOVIE_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_LIST_MOVIE_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_LIST_MOVIE_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_LIST_MOVIE_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  },

  // [CLEAR_ADD_MOVIE]: function(state) {
  //   const {
  //     defaults,
  //     view,
  //     ...otherDefaultState
  //   } = defaultState;

  //   return Object.assign({}, state, otherDefaultState);
  // }

  [CLEAR_ADD_MOVIE]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState, section);
