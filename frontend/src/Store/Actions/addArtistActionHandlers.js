import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewArtist from 'Utilities/Artist/getNewArtist';
import * as types from './actionTypes';
import { set, update, updateItem } from './baseActions';

let abortCurrentRequest = null;
const section = 'addArtist';

const addArtistActionHandlers = {
  [types.LOOKUP_ARTIST]: function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      if (abortCurrentRequest) {
        abortCurrentRequest();
      }

      const { request, abortRequest } = createAjaxRequest({
        url: '/artist/lookup',
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
    };
  },

  [types.ADD_ARTIST]: function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isAdding: true }));

      const foreignArtistId = payload.foreignArtistId;
      const items = getState().addArtist.items;
      const newSeries = getNewArtist(_.cloneDeep(_.find(items, { foreignArtistId })), payload);

      const promise = $.ajax({
        url: '/artist',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newSeries)
      });

      promise.done((data) => {
        dispatch(batchActions([
          updateItem({ section: 'artist', ...data }),

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
    };
  }
};

export default addArtistActionHandlers;
