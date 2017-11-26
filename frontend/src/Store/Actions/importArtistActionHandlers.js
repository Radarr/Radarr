import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import getNewArtist from 'Utilities/Artist/getNewArtist';
import * as types from './actionTypes';
import { set, updateItem, removeItem } from './baseActions';
import { startLookupArtist } from './importArtistActions';
import { fetchRootFolders } from './rootFolderActions';

const section = 'importArtist';
let concurrentLookups = 0;

const importArtistActionHandlers = {
  [types.QUEUE_LOOKUP_ARTIST]: function(payload) {
    return function(dispatch, getState) {
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
    };
  },

  [types.START_LOOKUP_ARTIST]: function(payload) {
    return function(dispatch, getState) {
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
    };
  },

  [types.IMPORT_ARTIST]: function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isImporting: true }));

      const ids = payload.ids;
      const items = getState().importArtist.items;
      const addedIds = [];

      const allNewSeries = ids.reduce((acc, id) => {
        const item = _.find(items, { id });
        const selectedArtist = item.selectedArtist;

        // Make sure we have a selected artist and
        // the same artist hasn't been added yet.
        if (selectedArtist && !_.some(acc, { foreignArtistId: selectedArtist.foreignArtistId })) {
          const newSeries = getNewArtist(_.cloneDeep(selectedArtist), item);
          newSeries.path = item.path;

          addedIds.push(id);
          acc.push(newSeries);
        }

        return acc;
      }, []);

      const promise = $.ajax({
        url: '/artist/import',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(allNewSeries)
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
    };
  }
};

export default importArtistActionHandlers;
