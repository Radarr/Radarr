import _ from 'lodash';
import moment from 'moment/moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByProp from 'Utilities/Array/sortByProp';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';
import { removeItem, set, update, updateItem } from './baseActions';
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
    includeRecommendations: true,
    includeTrending: true,
    includePopular: true
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    minimumAvailability: 'released',
    searchForMovie: true,
    tags: []
  },

  posterOptions: {
    size: 'large',
    showTitle: false,
    showTmdbRating: false,
    showImdbRating: false,
    showRottenTomatoesRating: false
  },

  overviewOptions: {
    size: 'medium',
    showYear: true,
    showStudio: true,
    showGenres: true,
    showTmdbRating: false,
    showImdbRating: false,
    showCertification: true
  },

  tableOptions: {
    // showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: () => translate('Status'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'isRecommendation',
      columnLabel: () => translate('Recommendation'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'isTrending',
      columnLabel: () => translate('Trending'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'isPopular',
      columnLabel: () => translate('Popular'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: () => translate('MovieTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'collection',
      label: () => translate('Collection'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'studio',
      label: () => translate('Studio'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'inCinemas',
      label: () => translate('InCinemas'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'digitalRelease',
      label: () => translate('DigitalRelease'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'physicalRelease',
      label: () => translate('PhysicalRelease'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'runtime',
      label: () => translate('Runtime'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tmdbRating',
      label: () => translate('TmdbRating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'imdbRating',
      label: () => translate('ImdbRating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'rottenTomatoesRating',
      label: () => translate('RottenTomatoesRating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'popularity',
      label: () => translate('Popularity'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'certification',
      label: () => translate('Certification'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'lists',
      label: () => translate('Lists'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
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

      return collection.title;
    },

    originalLanguage: function(item) {
      const { originalLanguage ={} } = item;

      return originalLanguage.name;
    },

    studio: function(item) {
      const studio = item.studio;

      return studio ? studio.toLowerCase() : '';
    },

    inCinemas: function(item, direction) {
      if (item.inCinemas) {
        return moment(item.inCinemas).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return -1 * Number.MAX_VALUE;
      }

      return Number.MAX_VALUE;
    },

    physicalRelease: function(item, direction) {
      if (item.physicalRelease) {
        return moment(item.physicalRelease).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return -1 * Number.MAX_VALUE;
      }

      return Number.MAX_VALUE;
    },

    digitalRelease: function(item, direction) {
      if (item.digitalRelease) {
        return moment(item.digitalRelease).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return -1 * Number.MAX_VALUE;
      }

      return Number.MAX_VALUE;
    },

    tmdbRating: function({ ratings = {} }) {
      return ratings.tmdb ? ratings.tmdb.value : 0;
    },

    imdbRating: function({ ratings = {} }) {
      return ratings.imdb ? ratings.imdb.value : 0;
    },

    rottenTomatoesRating: function({ ratings = {} }) {
      return ratings.rottenTomatoes ? ratings.rottenTomatoes.value : -1;
    }
  },

  selectedFilterKey: 'newNotExcluded',

  filters: [
    {
      key: 'all',
      label: () => translate('All'),
      filters: []
    },
    {
      key: 'popular',
      label: () => translate('Popular'),
      filters: [
        {
          key: 'isPopular',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'trending',
      label: () => translate('Trending'),
      filters: [
        {
          key: 'isTrending',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'newNotExcluded',
      label: () => translate('NewNonExcluded'),
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
      label: () => translate('ReleaseStatus'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.RELEASE_STATUS
    },
    {
      name: 'studio',
      label: () => translate('Studio'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, movie) => {
          acc.push({
            id: movie.studio,
            name: movie.studio
          });

          return acc;
        }, []);

        return tagList.sort(sortByProp('name'));
      }
    },
    {
      name: 'collection',
      label: () => translate('Collection'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, movie) => {
          if (movie.collection && movie.collection.title) {
            acc.push({
              id: movie.collection.title,
              name: movie.collection.title
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByProp('name'));
      }
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, movie) => {
          if (movie.originalLanguage) {
            acc.push({
              id: movie.originalLanguage.name,
              name: movie.originalLanguage.name
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByProp('name'));
      }
    },
    {
      name: 'inCinemas',
      label: () => translate('InCinemas'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'physicalRelease',
      label: () => translate('PhysicalRelease'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'digitalRelease',
      label: () => translate('DigitalRelease'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'runtime',
      label: () => translate('Runtime'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
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

        return tagList.sort(sortByProp('name'));
      }
    },
    {
      name: 'isAvailable',
      label: () => translate('ConsideredAvailable'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'minimumAvailability',
      label: () => translate('MinimumAvailability'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.MINIMUM_AVAILABILITY
    },
    {
      name: 'tmdbRating',
      label: () => translate('TmdbRating'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'tmdbVotes',
      label: () => translate('TmdbVotes'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'imdbRating',
      label: () => translate('ImdbRating'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'imdbVotes',
      label: () => translate('ImdbVotes'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'popularity',
      label: () => translate('Popularity'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'certification',
      label: () => translate('Certification'),
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'lists',
      label: () => translate('Lists'),
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.IMPORTLIST
    },
    {
      name: 'isExcluded',
      label: () => translate('OnExcludedList'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isExisting',
      label: () => translate('ExistsInLibrary'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isRecommendation',
      label: () => translate('Recommended'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isTrending',
      label: () => translate('Trending'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'isPopular',
      label: () => translate('Popular'),
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

export const ADD_IMPORT_LIST_EXCLUSIONS = 'discoverMovie/addImportListExclusions';

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

export const addImportListExclusions = createThunk(ADD_IMPORT_LIST_EXCLUSIONS);

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

    const {
      includeRecommendations = false,
      includeTrending = false,
      includePopular = false
    } = getState().discoverMovie.options;

    const promise = createAjaxRequest({
      url: `/importlist/movie?includeRecommendations=${includeRecommendations}&includeTrending=${includeTrending}&includePopular=${includePopular}`,
      data: otherPayload,
      traditional: true
    }).request;

    promise.done((data) => {
      // set an ID so the selectors and updaters done blow up.
      data = data.map((movie) => ({ ...movie, id: movie.tmdbId }));

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
      url: '/importlist/movie',
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

  [ADD_IMPORT_LIST_EXCLUSIONS]: function(getState, payload, dispatch) {

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
        ...data.map((item) => updateItem({ section: 'settings.importListExclusions', ...item })),

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
