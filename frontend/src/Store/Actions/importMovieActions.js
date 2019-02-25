import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set, removeItem, updateItem } from './baseActions';
import { fetchRootFolders } from './rootFolderActions';

//
// Variables

export const section = 'importMovie';
let concurrentLookups = 0;
let abortCurrentLookup = null;
const queue = [];

//
// State

export const defaultState = {
  isLookingUpMovie: false,
  isImporting: false,
  isImported: false,
  importError: null,
  items: []
};

//
// Actions Types

export const QUEUE_LOOKUP_MOVIE = 'importMovie/queueLookupMovie';
export const START_LOOKUP_MOVIE = 'importMovie/startLookupMovie';
export const CANCEL_LOOKUP_MOVIE = 'importMovie/cancelLookupMovie';
export const CLEAR_IMPORT_MOVIE = 'importMovie/clearImportMovie';
export const SET_IMPORT_MOVIE_VALUE = 'importMovie/setImportMovieValue';
export const IMPORT_MOVIE = 'importMovie/importMovie';

//
// Action Creators

export const queueLookupMovie = createThunk(QUEUE_LOOKUP_MOVIE);
export const startLookupMovie = createThunk(START_LOOKUP_MOVIE);
export const importMovie = createThunk(IMPORT_MOVIE);
export const clearImportMovie = createAction(CLEAR_IMPORT_MOVIE);
export const cancelLookupMovie = createAction(CANCEL_LOOKUP_MOVIE);

export const setImportMovieValue = createAction(SET_IMPORT_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [QUEUE_LOOKUP_MOVIE]: function(getState, payload, dispatch) {
    const {
      name,
      path,
      term,
      topOfQueue = false
    } = payload;

    const state = getState().importMovie;
    const item = _.find(state.items, { id: name }) || {
      id: name,
      term,
      path,
      isFetching: false,
      isPopulated: false,
      error: null
    };

    dispatch(updateItem({
      section,
      ...item,
      term,
      queued: true,
      items: []
    }));

    const itemIndex = queue.indexOf(item.id);

    if (itemIndex >= 0) {
      queue.splice(itemIndex, 1);
    }

    if (topOfQueue) {
      queue.unshift(item.id);
    } else {
      queue.push(item.id);
    }

    if (term && term.length > 2) {
      dispatch(startLookupMovie({ start: true }));
    }
  },

  [START_LOOKUP_MOVIE]: function(getState, payload, dispatch) {
    if (concurrentLookups >= 1) {
      return;
    }

    const state = getState().importMovie;

    const {
      isLookingUpMovie,
      items
    } = state;

    const queueId = queue[0];

    if (payload.start && !isLookingUpMovie) {
      dispatch(set({ section, isLookingUpMovie: true }));
    } else if (!isLookingUpMovie) {
      return;
    } else if (!queueId) {
      dispatch(set({ section, isLookingUpMovie: false }));
      return;
    }

    concurrentLookups++;
    queue.splice(0, 1);

    const queued = items.find((i) => i.id === queueId);

    dispatch(updateItem({
      section,
      id: queued.id,
      isFetching: true
    }));

    const { request, abortRequest } = createAjaxRequest({
      url: '/movie/lookup',
      data: {
        term: queued.term
      }
    });

    abortCurrentLookup = abortRequest;

    request.done((data) => {
      dispatch(updateItem({
        section,
        id: queued.id,
        isFetching: false,
        isPopulated: true,
        error: null,
        items: data,
        queued: false,
        selectedMovie: queued.selectedMovie || data[0],
        updateOnly: true
      }));
    });

    request.fail((xhr) => {
      dispatch(updateItem({
        section,
        id: queued.id,
        isFetching: false,
        isPopulated: false,
        error: xhr,
        queued: false,
        updateOnly: true
      }));
    });

    request.always(() => {
      concurrentLookups--;

      dispatch(startLookupMovie());
    });
  },

  [IMPORT_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isImporting: true }));

    const ids = payload.ids;
    const items = getState().importMovie.items;
    const addedIds = [];

    const allNewMovies = ids.reduce((acc, id) => {
      const item = _.find(items, { id });
      const selectedMovie = item.selectedMovie;

      // Make sure we have a selected series and
      // the same series hasn't been added yet.
      if (selectedMovie && !_.some(acc, { tmdbId: selectedMovie.tmdbId })) {
        const newSeries = getNewMovie(_.cloneDeep(selectedMovie), item);
        newSeries.path = item.path;

        addedIds.push(id);
        acc.push(newSeries);
      }

      return acc;
    }, []);

    const promise = $.ajax({
      url: '/movie/import',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(allNewMovies)
    });

    promise.done((data) => {
      dispatch(batchActions([
        set({
          section,
          isImporting: false,
          isImported: true
        }),

        ...data.map((series) => updateItem({ section: 'movies', ...series })),

        ...addedIds.map((id) => removeItem({ section, id }))
      ]));

      dispatch(fetchRootFolders());
    });

    promise.fail((xhr) => {
      dispatch(batchActions(
        set({
          section,
          isImporting: false,
          isImported: true
        }),

        addedIds.map((id) => updateItem({
          section,
          id,
          importError: xhr
        }))
      ));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CANCEL_LOOKUP_MOVIE]: function(state) {
    return Object.assign({}, state, { isLookingUpMovie: false });
  },

  [CLEAR_IMPORT_MOVIE]: function(state) {
    if (abortCurrentLookup) {
      abortCurrentLookup();

      abortCurrentLookup = null;
    }

    queue.splice(0, queue.length);

    return Object.assign({}, state, defaultState);
  },

  [SET_IMPORT_MOVIE_VALUE]: function(state, { payload }) {
    const newState = getSectionState(state, section);
    const items = newState.items;
    const index = _.findIndex(items, { id: payload.id });

    newState.items = [...items];

    if (index >= 0) {
      const item = items[index];

      newState.items.splice(index, 1, { ...item, ...payload });
    } else {
      newState.items.push({ ...payload });
    }

    return updateSectionState(state, section, newState);
  }

}, defaultState, section);
