import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByName from 'Utilities/Array/sortByName';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';
import { removeItem, set, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterPredicates } from './movieActions';

//
// Variables

export const section = 'discoverMovie';

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

  options: {
    includeRecommendations: true
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'true',
    qualityProfileId: 0,
    minimumAvailability: 'announced',
    searchForMovie: true,
    tags: []
  },

  posterOptions: {
    size: 'large',
    showTitle: false
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showStudio: true,
    showRatings: true,
    showYear: true,
    showCertification: true,
    showGenres: true
  },

  tableOptions: {
    // showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: translate('Status'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'isRecommendation',
      columnLabel: 'Recommedation',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: translate('MovieTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'collection',
      label: translate('Collection'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'studio',
      label: translate('Studio'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'inCinemas',
      label: translate('InCinemas'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'physicalRelease',
      label: translate('PhysicalRelease'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'digitalRelease',
      label: translate('DigitalRelease'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'runtime',
      label: translate('Runtime'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'ratings',
      label: translate('Ratings'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'certification',
      label: translate('Certification'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'lists',
      label: 'Lists',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ],

  sortPredicates: {
    status: function(item) {
      let result = 0;

      if (item.isExcluded) {
        result += 4;
      }

      if (item.status === 'announced') {
        result++;
      }

      if (item.status === 'inCinemas') {
        result += 2;
      }

      if (item.status === 'released') {
        result += 3;
      }

      return result;
    },

    collection: function(item) {
      const { collection ={} } = item;

      return collection.name;
    },

    studio: function(item) {
      const studio = item.studio;

      return studio ? studio.toLowerCase() : '';
    },

    ratings: function(item) {
      const { ratings = {} } = item;

      return ratings.value;
    }
  },

  selectedFilterKey: 'newNotExcluded',

  filters: [
    {
      key: 'all',
      label: translate('All'),
      filters: []
    },
    {
      key: 'newNotExcluded',
      label: 'New Non-Excluded',
      filters: [
        {
          key: 'isExisting',
          value: false,
          type: filterTypes.EQUAL
        },
        {
          key: 'isExcluded',
          value: false,
          type: filterTypes.EQUAL
        }
      ]
    }
  ],

  filterPredicates,

  filterBuilderProps: [
    {
      name: 'status',
      label: translate('ReleaseStatus'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.RELEASE_STATUS
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
      name: 'collection',
      label: translate('Collection'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, movie) => {
          if (movie.collection) {
            acc.push({
              id: movie.collection.name,
              name: movie.collection.name
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByName);
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
      name: 'digitalRelease',
      label: 'Digital Release',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'runtime',
      label: translate('Runtime'),
      type: filterBuilderTypes.NUMBER
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
      name: 'isAvailable',
      label: translate('ConsideredAvailable'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'minimumAvailability',
      label: translate('MinimumAvailability'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.MINIMUM_AVAILABILITY
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
      name: 'lists',
      label: 'Lists',
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.IMPORTLIST
    },
    {
      name: 'isExcluded',
      label: 'On Excluded List',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isExisting',
      label: 'Exists in Library',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isRecommendation',
      label: 'Recommended',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    }
  ]
};

export const persistState = [
  'discoverMovie.defaults',
  'discoverMovie.sortKey',
  'discoverMovie.sortDirection',
  'discoverMovie.selectedFilterKey',
  'discoverMovie.customFilters',
  'discoverMovie.view',
  'discoverMovie.columns',
  'discoverMovie.options',
  'discoverMovie.posterOptions',
  'discoverMovie.overviewOptions',
  'discoverMovie.tableOptions'
];

//
// Actions Types

export const ADD_MOVIE = 'discoverMovie/addMovie';
export const ADD_MOVIES = 'discoverMovie/addMovies';
export const SET_ADD_MOVIE_VALUE = 'discoverMovie/setAddMovieValue';
export const CLEAR_ADD_MOVIE = 'discoverMovie/clearAddMovie';
export const SET_ADD_MOVIE_DEFAULT = 'discoverMovie/setAddMovieDefault';

export const FETCH_DISCOVER_MOVIES = 'discoverMovie/fetchDiscoverMovies';

export const SET_LIST_MOVIE_SORT = 'discoverMovie/setListMovieSort';
export const SET_LIST_MOVIE_FILTER = 'discoverMovie/setListMovieFilter';
export const SET_LIST_MOVIE_VIEW = 'discoverMovie/setListMovieView';
export const SET_LIST_MOVIE_OPTION = 'discoverMovie/setListMovieMovieOption';
export const SET_LIST_MOVIE_TABLE_OPTION = 'discoverMovie/setListMovieTableOption';
export const SET_LIST_MOVIE_POSTER_OPTION = 'discoverMovie/setListMoviePosterOption';
export const SET_LIST_MOVIE_OVERVIEW_OPTION = 'discoverMovie/setListMovieOverviewOption';

export const ADD_IMPORT_EXCLUSIONS = 'discoverMovie/addImportExclusions';

//
// Action Creators

export const addMovie = createThunk(ADD_MOVIE);
export const addMovies = createThunk(ADD_MOVIES);
export const clearAddMovie = createAction(CLEAR_ADD_MOVIE);
export const setAddMovieDefault = createAction(SET_ADD_MOVIE_DEFAULT);

export const fetchDiscoverMovies = createThunk(FETCH_DISCOVER_MOVIES);

export const setListMovieSort = createAction(SET_LIST_MOVIE_SORT);
export const setListMovieFilter = createAction(SET_LIST_MOVIE_FILTER);
export const setListMovieView = createAction(SET_LIST_MOVIE_VIEW);
export const setListMovieOption = createAction(SET_LIST_MOVIE_OPTION);
export const setListMovieTableOption = createAction(SET_LIST_MOVIE_TABLE_OPTION);
export const setListMoviePosterOption = createAction(SET_LIST_MOVIE_POSTER_OPTION);
export const setListMovieOverviewOption = createAction(SET_LIST_MOVIE_OVERVIEW_OPTION);

export const addImportExclusions = createThunk(ADD_IMPORT_EXCLUSIONS);

export const setAddMovieValue = createAction(SET_ADD_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_DISCOVER_MOVIES]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const {
      id,
      ...otherPayload
    } = payload;

    const includeRecommendations = getState().discoverMovie.options.includeRecommendations;

    const promise = createAjaxRequest({
      url: `/importlist/movie?includeRecommendations=${includeRecommendations}`,
      data: otherPayload,
      traditional: true
    }).request;

    promise.done((data) => {
      // set an Id so the selectors and updaters done blow up.
      data = data.map((movie) => ({ ...movie, id: movie.tmdbId }));

      dispatch(batchActions([
        ...data.map((movie) => updateItem({ section, ...movie })),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    promise.fail((xhr) => {
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
    const items = getState().discoverMovie.items;
    const itemToUpdate = _.find(items, { tmdbId });

    const newMovie = getNewMovie(_.cloneDeep(itemToUpdate), payload);
    newMovie.id = 0;

    const promise = createAjaxRequest({
      url: '/movie',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(newMovie)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'movies', ...data }),

        itemToUpdate.lists.length === 0 ? removeItem({ section: 'discoverMovie', ...itemToUpdate }) :
          updateItem({ section: 'discoverMovie', ...itemToUpdate, isExisting: true }),

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
  },

  [ADD_MOVIES]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const ids = payload.ids;
    const addOptions = payload.addOptions;
    const items = getState().discoverMovie.items;
    const addedIds = [];

    const allNewMovies = ids.reduce((acc, id) => {
      const item = items.find((i) => i.id === id);
      const selectedMovie = item;

      // Make sure we have a selected movie and
      // the same movie hasn't been added yet.
      if (selectedMovie && !acc.some((a) => a.tmdbId === selectedMovie.tmdbId)) {
        if (!selectedMovie.isExisting) {
          const newMovie = getNewMovie(_.cloneDeep(selectedMovie), addOptions);
          newMovie.id = 0;

          addedIds.push(id);
          acc.push(newMovie);
        }
      }

      return acc;
    }, []);

    const promise = createAjaxRequest({
      url: '/movie/import',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(allNewMovies)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        set({
          section,
          isAdding: false,
          isAdded: true
        }),

        ...data.map((movie) => updateItem({ section: 'movies', ...movie })),

        ...addedIds.map((id) => (items.find((i) => i.id === id).lists.length === 0 ? removeItem({ section, id }) : updateItem({ section, id, isExisting: true })))

      ]));
    });

    promise.fail((xhr) => {
      dispatch(
        set({
          section,
          isAdding: false,
          isAdded: true
        })
      );
    });
  },

  [ADD_IMPORT_EXCLUSIONS]: function(getState, payload, dispatch) {

    const ids = payload.ids;
    const items = getState().discoverMovie.items;

    const exclusions = ids.reduce((acc, id) => {
      const item = items.find((i) => i.tmdbId === id);

      const newExclusion = {
        tmdbId: id,
        movieTitle: item.title,
        movieYear: item.year
      };

      acc.push(newExclusion);

      return acc;
    }, []);

    const promise = createAjaxRequest({
      url: '/exclusions/bulk',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(exclusions)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((item) => updateItem({ section: 'settings.importExclusions', ...item })),

        ...data.map((item) => updateItem({ section, id: item.tmdbId, isExcluded: true })),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
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

  [SET_LIST_MOVIE_OPTION]: function(state, { payload }) {
    const discoveryMovieOptions = state.options;

    return {
      ...state,
      options: {
        ...discoveryMovieOptions,
        ...payload
      }
    };
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

  [CLEAR_ADD_MOVIE]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState, section);
