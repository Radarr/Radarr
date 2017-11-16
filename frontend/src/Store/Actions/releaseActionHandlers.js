import $ from 'jquery';
import createFetchHandler from './Creators/createFetchHandler';
import * as types from './actionTypes';
import { updateRelease } from './releaseActions';

let abortCurrentRequest = null;
const section = 'releases';

const fetchReleases = createFetchHandler(section, '/release');

const releaseActionHandlers = {
  [types.FETCH_RELEASES]: function(payload) {
    return function(dispatch, getState) {
      const abortRequest = fetchReleases(payload)(dispatch, getState);

      abortCurrentRequest = abortRequest;
    };
  },

  [types.CANCEL_FETCH_RELEASES]: function(payload) {
    return function(dispatch, getState) {
      if (abortCurrentRequest) {
        abortCurrentRequest = abortCurrentRequest();
      }
    };
  },

  [types.GRAB_RELEASE]: function(payload) {
    return function(dispatch, getState) {
      const guid = payload.guid;

      dispatch(updateRelease({ guid, isGrabbing: true }));

      const promise = $.ajax({
        url: '/release',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
      });

      promise.done((data) => {
        dispatch(updateRelease({
          guid,
          isGrabbing: false,
          isGrabbed: true,
          grabError: null
        }));
      });

      promise.fail((xhr) => {
        const grabError = xhr.responseJSON && xhr.responseJSON.message || 'Failed to add to download queue';

        dispatch(updateRelease({
          guid,
          isGrabbing: false,
          isGrabbed: false,
          grabError
        }));
      });
    };
  }
};

export default releaseActionHandlers;
