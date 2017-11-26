import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
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

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isImporting: false,
  isImported: false,
  importError: null,
  items: []
};

//
// Actions Types

export const QUEUE_LOOKUP_ARTIST = 'importArtist/queueLookupArtist';
export const START_LOOKUP_ARTIST = 'importArtist/startLookupArtist';
export const CLEAR_IMPORT_ARTIST = 'importArtist/importArtist';
export const SET_IMPORT_ARTIST_VALUE = 'importArtist/clearImportArtist';
export const IMPORT_ARTIST = 'importArtist/setImportArtistValue';

//
// Action Creators

export const queueLookupArtist = createThunk(QUEUE_LOOKUP_ARTIST);
export const startLookupArtist = createThunk(START_LOOKUP_ARTIST);
export const importArtist = createThunk(IMPORT_ARTIST);
export const clearImportArtist = createAction(CLEAR_IMPORT_ARTIST);

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
      term
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
      queued: true,
      items: []
    }));

    if (term && term.length > 2) {
      dispatch(startLookupArtist());
    }
  },

  [START_LOOKUP_ARTIST]: function(getState, payload, dispatch) {
    if (concurrentLookups >= 1) {
      return;
    }

    const state = getState().importArtist;
    const queued = _.find(state.items, { queued: true });

    if (!queued) {
      return;
    }

    concurrentLookups++;

    dispatch(updateItem({
      section,
      id: queued.id,
      isFetching: true
    }));

    const promise = $.ajax({
      url: '/artist/lookup',
      data: {
        term: queued.term
      }
    });

    promise.done((data) => {
      dispatch(updateItem({
        section,
        id: queued.id,
        isFetching: false,
        isPopulated: true,
        error: null,
        items: data,
        queued: false,
        selectedArtist: queued.selectedArtist || data[0]
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        section,
        id: queued.id,
        isFetching: false,
        isPopulated: false,
        error: xhr,
        queued: false
      }));
    });

    promise.always(() => {
      concurrentLookups--;
      dispatch(startLookupArtist());
    });
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
      if (selectedArtist && !_.some(acc, { tvdbId: selectedArtist.tvdbId })) {
        const newArtist = getNewArtist(_.cloneDeep(selectedArtist), item);
        newArtist.path = item.path;

        addedIds.push(id);
        acc.push(newArtist);
      }

      return acc;
    }, []);

    const promise = $.ajax({
      url: '/artist/import',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(allNewArtist)
    });

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

  [CLEAR_IMPORT_ARTIST]: function(state) {
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
