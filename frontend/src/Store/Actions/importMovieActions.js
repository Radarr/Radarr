import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';
import { removeItem, set, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import { fetchRootFolders } from './rootFolderActions';

//
// Variables

export const section = 'importMovie';
let concurrentLookups = 0;
let abortCurrentLookup = null;
const queue = [];
const COMPLETION_DISPLAY_TIMEOUT = 1500;

//
// State

export const defaultState = {
  isLookingUpMovie: false,
  isImporting: false,
  isImported: false,
  importError: null,
  importingCount: 0,
  importedCount: 0,
  items: []
};

//
// Actions Types

export const QUEUE_LOOKUP_MOVIE = 'importMovie/queueLookupMovie';
export const START_LOOKUP_MOVIE = 'importMovie/startLookupMovie';
export const CANCEL_LOOKUP_MOVIE = 'importMovie/cancelLookupMovie';
export const LOOKUP_UNSEARCHED_MOVIES = 'importMovie/lookupUnsearchedMovies';
export const CLEAR_IMPORT_MOVIE = 'importMovie/clearImportMovie';
export const SET_IMPORT_MOVIE_VALUE = 'importMovie/setImportMovieValue';
export const IMPORT_MOVIE = 'importMovie/importMovie';

//
// Action Creators

export const queueLookupMovie = createThunk(QUEUE_LOOKUP_MOVIE);
export const startLookupMovie = createThunk(START_LOOKUP_MOVIE);
export const importMovie = createThunk(IMPORT_MOVIE);
export const lookupUnsearchedMovies = createThunk(LOOKUP_UNSEARCHED_MOVIES);
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
      relativePath,
      term,
      topOfQueue = false
    } = payload;

    const state = getState().importMovie;
    const item = _.find(state.items, { id: name }) || {
      id: name,
      term,
      path,
      relativePath,
      isFetching: false,
      isPopulated: false,
      error: null
    };

    dispatch(updateItem({
      section,
      ...item,
      term,
      isQueued: true,
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
      const selectedMovie = queued.selectedMovie || data[0];

      dispatch(updateItem({
        section,
        id: queued.id,
        isFetching: false,
        isPopulated: true,
        error: null,
        items: data,
        isQueued: false,
        selectedMovie,
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
        isQueued: false,
        updateOnly: true
      }));
    });

    request.always(() => {
      concurrentLookups--;

      dispatch(startLookupMovie());
    });
  },

  [LOOKUP_UNSEARCHED_MOVIES]: function(getState, payload, dispatch) {
    const state = getState().importMovie;

    if (state.isLookingUpMovie) {
      return;
    }

    state.items.forEach((item) => {
      const id = item.id;

      if (
        !item.isPopulated &&
        !queue.includes(id)
      ) {
        queue.push(item.id);
      }
    });

    if (queue.length) {
      dispatch(startLookupMovie({ start: true }));
    }
  },

  [IMPORT_MOVIE]: function(getState, payload, dispatch) {
    const ids = payload.ids;
    const items = getState().importMovie.items;
    const addedIds = [];

    const allNewMovies = ids.reduce((acc, id) => {
      const item = items.find((i) => i.id === id);
      const selectedMovie = item.selectedMovie;

      // Make sure we have a selected movie and
      // the same movie hasn't been added yet.
      if (selectedMovie && !acc.some((a) => a.tmdbId === selectedMovie.tmdbId)) {
        const newMovie = getNewMovie(_.cloneDeep(selectedMovie), item);
        newMovie.path = item.path;

        addedIds.push(id);
        acc.push(newMovie);
      }

      return acc;
    }, []);

    dispatch(set({
      section,
      isImporting: true,
      importingCount: allNewMovies.length,
      importedCount: 0
    }));

    let importedCount = 0;
    const importedMovies = [];
    let hasError = false;

    const importNextMovie = (index) => {
      if (index >= allNewMovies.length || hasError) {
        setTimeout(() => {
          dispatch(batchActions([
            set({
              section,
              isImporting: false,
              isImported: true,
              importError: hasError ? { message: translate('ImportErrors') } : null,
              importedCount
            }),

            ...importedMovies.map((movie) => updateItem({ section: 'movies', ...movie })),

            ...addedIds.slice(0, importedCount).map((id) => removeItem({ section, id }))
          ]));

          dispatch(fetchRootFolders());
        }, COMPLETION_DISPLAY_TIMEOUT);
        return;
      }

      const movieToImport = allNewMovies[index];

      const promise = createAjaxRequest({
        url: '/movie/import',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify([movieToImport])
      }).request;

      promise.done((data) => {
        importedCount++;
        if (data && data.length > 0) {
          importedMovies.push(...data);
        }

        dispatch(set({
          section,
          importedCount
        }));

        importNextMovie(index + 1);
      });

      promise.fail(() => {
        hasError = true;

        setTimeout(() => {
          dispatch(batchActions([
            set({
              section,
              isImporting: false,
              isImported: true,
              importError: { message: translate('ImportErrors') },
              importedCount
            }),

            ...importedMovies.map((movie) => updateItem({ section: 'movies', ...movie }))
          ]));
        }, COMPLETION_DISPLAY_TIMEOUT);
      });
    };

    importNextMovie(0);
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CANCEL_LOOKUP_MOVIE]: function(state) {
    queue.splice(0, queue.length);

    const items = state.items.map((item) => {
      if (item.isQueued) {
        return {
          ...item,
          isQueued: false
        };
      }

      return item;
    });

    return Object.assign({}, state, {
      isLookingUpMovie: false,
      items
    });
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
    const index = items.findIndex((item) => item.id === payload.id);

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
