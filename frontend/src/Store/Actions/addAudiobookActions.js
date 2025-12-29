import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { set, update, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'addAudiobook';
let abortCurrentRequest = null;

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
    monitor: true,
    qualityProfileId: 0,
    searchForAudiobook: true,
    tags: []
  }
};

export const persistState = [
  'addAudiobook.defaults'
];

export const LOOKUP_AUDIOBOOK = 'addAudiobook/lookupAudiobook';
export const ADD_AUDIOBOOK = 'addAudiobook/addAudiobook';
export const SET_ADD_AUDIOBOOK_VALUE = 'addAudiobook/setAddAudiobookValue';
export const CLEAR_ADD_AUDIOBOOK = 'addAudiobook/clearAddAudiobook';
export const SET_ADD_AUDIOBOOK_DEFAULT = 'addAudiobook/setAddAudiobookDefault';

export const lookupAudiobook = createThunk(LOOKUP_AUDIOBOOK);
export const addAudiobook = createThunk(ADD_AUDIOBOOK);
export const clearAddAudiobook = createAction(CLEAR_ADD_AUDIOBOOK);
export const setAddAudiobookDefault = createAction(SET_ADD_AUDIOBOOK_DEFAULT);

export const setAddAudiobookValue = createAction(SET_ADD_AUDIOBOOK_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({

  [LOOKUP_AUDIOBOOK]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/audiobook/lookup',
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

  [ADD_AUDIOBOOK]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const id = payload.id;
    const items = getState().addAudiobook.items;
    const found = _.find(items, { id });
    const newAudiobook = {
      ...structuredClone(found),
      ...payload,
      id: 0
    };

    const promise = createAjaxRequest({
      url: '/audiobook',
      method: 'POST',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify(newAudiobook)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'audiobooks', ...data }),

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

export const reducers = createHandleActions({

  [SET_ADD_AUDIOBOOK_VALUE]: createSetSettingValueReducer(section),

  [SET_ADD_AUDIOBOOK_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [CLEAR_ADD_AUDIOBOOK]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return { ...state, ...otherDefaultState };
  }

}, defaultState, section);
