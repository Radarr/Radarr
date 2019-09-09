import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getProviderState from 'Utilities/State/getProviderState';
import { set, updateItem, removeItem } from '../baseActions';

const abortCurrentRequests = {};

export function createCancelSaveProviderHandler(section) {
  return function(getState, payload, dispatch) {
    if (abortCurrentRequests[section]) {
      abortCurrentRequests[section]();
      abortCurrentRequests[section] = null;
    }
  };
}

function createSaveProviderHandler(section, url, options = {}, removeStale = false) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isSaving: true }));

    const {
      id,
      queryParams = {},
      ...otherPayload
    } = payload;

    const saveData = Array.isArray(id) ? id.map((x) => getProviderState({ id: x, ...otherPayload }, getState, section)) : getProviderState({ id, ...otherPayload }, getState, section);

    const ajaxOptions = {
      url: `${url}?${$.param(queryParams, true)}`,
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify(saveData)
    };

    if (id) {
      ajaxOptions.method = 'PUT';
      if (!Array.isArray(id)) {
        ajaxOptions.url = `${url}/${id}?${$.param(queryParams, true)}`;
      }
    }

    const { request, abortRequest } = createAjaxRequest(ajaxOptions);

    abortCurrentRequests[section] = abortRequest;

    request.done((data) => {
      if (!Array.isArray(data)) {
        data = [data];
      }

      const toRemove = removeStale && Array.isArray(id) ? _.difference(id, _.map(data, 'id')) : [];

      dispatch(batchActions(
        data.map((item) => updateItem({ section, ...item })).concat(
          toRemove.map((item) => removeItem({ section, id: item }))
        ).concat(
          set({
            section,
            isSaving: false,
            saveError: null,
            pendingChanges: {}
          })
        )));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr.aborted ? null : xhr
      }));
    });
  };
}

export default createSaveProviderHandler;
