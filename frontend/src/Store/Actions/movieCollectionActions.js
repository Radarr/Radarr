import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import { set, update, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

//
// Variables

export const section = 'movieCollections';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  isSaving: false,
  saveError: null,
  isAdding: false,
  addError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'overview',
  pendingChanges: {},

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showDetails: true,
    showOverview: true
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    minimumAvailability: 'announced',
    searchForMovie: true,
    tags: []
  },

  selectedFilterKey: 'all',

  filters: [
    {
      key: 'all',
      label: 'All',
      filters: []
    }
  ],

  filterPredicates: {},

  filterBuilderProps: [
    {
      name: 'title',
      label: 'Title',
      type: filterBuilderTypes.STRING
    },
    {
      name: 'monitored',
      label: 'Monitored',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    }
  ]
};

export const persistState = [
  'movieCollections.defaults',
  'movieCollections.sortKey',
  'movieCollections.sortDirection',
  'movieCollections.selectedFilterKey',
  'movieCollections.customFilters',
  'movieCollections.options',
  'movieCollections.overviewOptions'
];

//
// Actions Types

export const FETCH_MOVIE_COLLECTIONS = 'movieCollections/fetchMovieCollections';
export const CLEAR_MOVIE_COLLECTIONS = 'movieCollections/clearMovieCollections';
export const SAVE_MOVIE_COLLECTION = 'movieCollections/saveMovieCollection';
export const SAVE_MOVIE_COLLECTIONS = 'movieCollections/saveMovieCollections';
export const SET_MOVIE_COLLECTION_VALUE = 'movieCollections/setMovieCollectionValue';

export const ADD_MOVIE = 'movieCollections/addMovie';

export const TOGGLE_COLLECTION_MONITORED = 'movieCollections/toggleCollectionMonitored';

export const SET_MOVIE_COLLECTIONS_SORT = 'movieCollections/setMovieCollectionsSort';
export const SET_MOVIE_COLLECTIONS_FILTER = 'movieCollections/setMovieCollectionsFilter';
export const SET_MOVIE_COLLECTIONS_OPTION = 'movieCollections/setMovieCollectionsOption';
export const SET_MOVIE_COLLECTIONS_OVERVIEW_OPTION = 'movieCollections/setMovieCollectionsOverviewOption';

//
// Action Creators

export const fetchMovieCollections = createThunk(FETCH_MOVIE_COLLECTIONS);
export const clearMovieCollections = createAction(CLEAR_MOVIE_COLLECTIONS);
export const saveMovieCollection = createThunk(SAVE_MOVIE_COLLECTION);
export const saveMovieCollections = createThunk(SAVE_MOVIE_COLLECTIONS);

export const addMovie = createThunk(ADD_MOVIE);

export const toggleCollectionMonitored = createThunk(TOGGLE_COLLECTION_MONITORED);

export const setMovieCollectionsSort = createAction(SET_MOVIE_COLLECTIONS_SORT);
export const setMovieCollectionsFilter = createAction(SET_MOVIE_COLLECTIONS_FILTER);
export const setMovieCollectionsOption = createAction(SET_MOVIE_COLLECTIONS_OPTION);
export const setMovieCollectionsOverviewOption = createAction(SET_MOVIE_COLLECTIONS_OVERVIEW_OPTION);

export const setMovieCollectionValue = createAction(SET_MOVIE_COLLECTION_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [SAVE_MOVIE_COLLECTION]: createSaveProviderHandler(section, '/collection'),
  [FETCH_MOVIE_COLLECTIONS]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/collection',
      data: payload
    }).request;

    promise.done((data) => {
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
        error: xhr
      }));
    });
  },

  [ADD_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const tmdbId = payload.tmdbId;
    const title = payload.title;

    const newMovie = getNewMovie({ tmdbId, title }, payload);
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

  [TOGGLE_COLLECTION_MONITORED]: (getState, payload, dispatch) => {
    const {
      collectionId: id,
      monitored
    } = payload;

    const collection = _.find(getState().movieCollections.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/collection/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...collection,
        monitored
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false
      }));
    });
  },

  [SAVE_MOVIE_COLLECTIONS]: function(getState, payload, dispatch) {
    const {
      collectionIds,
      monitored,
      monitor
    } = payload;

    const response = {};
    const collections = [];

    collectionIds.forEach((id) => {
      const collectionToUpdate = { id };

      if (payload.hasOwnProperty('monitored')) {
        collectionToUpdate.monitored = monitored;
      }

      collections.push(collectionToUpdate);
    });

    if (payload.hasOwnProperty('monitor')) {
      response.monitorMovies = monitor === 'monitored';
    }

    response.collections = collections;

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/collection',
      method: 'PUT',
      data: JSON.stringify(response),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(fetchMovieCollections());

      dispatch(set({
        section,
        isSaving: false,
        saveError: null
      }));
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

  [SET_MOVIE_COLLECTIONS_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_MOVIE_COLLECTIONS_FILTER]: createSetClientSideCollectionFilterReducer(section),
  [SET_MOVIE_COLLECTION_VALUE]: createSetSettingValueReducer(section),

  [SET_MOVIE_COLLECTIONS_OPTION]: function(state, { payload }) {
    const movieCollectionsOptions = state.options;

    return {
      ...state,
      options: {
        ...movieCollectionsOptions,
        ...payload
      }
    };
  },

  [SET_MOVIE_COLLECTIONS_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  },

  [CLEAR_MOVIE_COLLECTIONS]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
