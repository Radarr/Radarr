import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import getNewArtist from 'Utilities/Artist/getNewArtist';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set, removeItem, updateItem } from './baseActions';
import { fetchRootFolders } from './rootFolderActions';

//
// Variables

export const section = 'importArtist';
let concurrentLookups = 0;
let abortCurrentLookup = null;
const queue = [];

//
// State

export const defaultState = {
  isLookingUpArtist: false,
  isImporting: false,
  isImported: false,
  importError: null,
  items: []
};

//
// Actions Types

export const QUEUE_LOOKUP_ARTIST = 'importArtist/queueLookupArtist';
export const START_LOOKUP_ARTIST = 'importArtist/startLookupArtist';
export const CANCEL_LOOKUP_ARTIST = 'importArtist/cancelLookupArtist';
export const LOOKUP_UNSEARCHED_ARTIST = 'importArtist/lookupUnsearchedArtist';
export const CLEAR_IMPORT_ARTIST = 'importArtist/clearImportArtist';
export const SET_IMPORT_ARTIST_VALUE = 'importArtist/setImportArtistValue';
export const IMPORT_ARTIST = 'importArtist/importArtist';

//
// Action Creators

export const queueLookupArtist = createThunk(QUEUE_LOOKUP_ARTIST);
export const startLookupArtist = createThunk(START_LOOKUP_ARTIST);
export const importArtist = createThunk(IMPORT_ARTIST);
export const lookupUnsearchedArtist = createThunk(LOOKUP_UNSEARCHED_ARTIST);
export const clearImportArtist = createAction(CLEAR_IMPORT_ARTIST);
export const cancelLookupArtist = createAction(CANCEL_LOOKUP_ARTIST);

export const setImportArtistValue = createAction(SET_IMPORT_ARTIST_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [QUEUE_LOOKUP_ARTIST]: function(getState, payload, dispatch) {
    const {
      name,
      path,
      term,
      topOfQueue = false
    } = payload;

    const state = getState().importArtist;
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
      dispatch(startLookupArtist({ start: true }));
    }
  },

  [START_LOOKUP_ARTIST]: function(getState, payload, dispatch) {
    if (concurrentLookups >= 1) {
      return;
    }

    const state = getState().importArtist;

    const {
      isLookingUpArtist,
      items
    } = state;

    const queueId = queue[0];

    if (payload.start && !isLookingUpArtist) {
      dispatch(set({ section, isLookingUpArtist: true }));
    } else if (!isLookingUpArtist) {
      return;
    } else if (!queueId) {
      dispatch(set({ section, isLookingUpArtist: false }));
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
      url: '/artist/lookup',
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
        isQueued: false,
        selectedArtist: queued.selectedArtist || data[0],
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

      dispatch(startLookupArtist());
    });
  },

  [LOOKUP_UNSEARCHED_ARTIST]: function(getState, payload, dispatch) {
    const state = getState().importArtist;

    if (state.isLookingUpArtist) {
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
      dispatch(startLookupArtist({ start: true }));
    }
  },

  [IMPORT_ARTIST]: function(getState, payload, dispatch) {
    dispatch(set({ section, isImporting: true }));

    const ids = payload.ids;
    const items = getState().importArtist.items;
    const addedIds = [];

    const allNewArtist = ids.reduce((acc, id) => {
      const item = _.find(items, { id });
      const selectedArtist = item.selectedArtist;

      // Make sure we have a selected artist and
      // the same artist hasn't been added yet.
      if (selectedArtist && !_.some(acc, { foreignArtistId: selectedArtist.foreignArtistId })) {
        const newArtist = getNewArtist(_.cloneDeep(selectedArtist), item);
        newArtist.path = item.path;

        addedIds.push(id);
        acc.push(newArtist);
      }

      return acc;
    }, []);

    const promise = createAjaxRequest({
      url: '/artist/import',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(allNewArtist)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        set({
          section,
          isImporting: false,
          isImported: true
        }),

        ...data.map((artist) => updateItem({ section: 'artist', ...artist })),

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

  [CANCEL_LOOKUP_ARTIST]: function(state) {
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
      isLookingUpArtist: false,
      items
    });
  },

  [CLEAR_IMPORT_ARTIST]: function(state) {
    if (abortCurrentLookup) {
      abortCurrentLookup();

      abortCurrentLookup = null;
    }

    queue.splice(0, queue.length);

    return Object.assign({}, state, defaultState);
  },

  [SET_IMPORT_ARTIST_VALUE]: function(state, { payload }) {
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
