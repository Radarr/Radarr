import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, update, updateItem } from './baseActions';

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

  defaults: {
    rootFolderPath: '',
    monitor: 'true',
    qualityProfileId: 0,
    tags: []
  }
};

export const persistState = [
  'addMovie.defaults'
];

//
// Actions Types

export const LOOKUP_MOVIE = 'addMovie/lookupMovie';
export const ADD_MOVIE = 'addMovie/addMovie';
export const SET_ADD_MOVIE_VALUE = 'addMovie/setAddMovieValue';
export const CLEAR_ADD_MOVIE = 'addMovie/clearAddMovie';
export const SET_ADD_MOVIE_DEFAULT = 'addMovie/setAddMovieDefault';

//
// Action Creators

export const lookupMovie = createThunk(LOOKUP_MOVIE);
export const addMovie = createThunk(ADD_MOVIE);
export const clearAddMovie = createAction(CLEAR_ADD_MOVIE);
export const setAddMovieDefault = createAction(SET_ADD_MOVIE_DEFAULT);

export const setAddMovieValue = createAction(SET_ADD_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

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
    const newSeries = getNewMovie(_.cloneDeep(_.find(items, { tmdbId })), payload);

    const promise = $.ajax({
      url: '/movie',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(newSeries)
    });

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

  [CLEAR_ADD_MOVIE]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState, section);
