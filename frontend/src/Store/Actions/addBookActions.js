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

export const section = 'addBook';
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
    searchForBook: true,
    tags: []
  }
};

export const persistState = [
  'addBook.defaults'
];

export const LOOKUP_BOOK = 'addBook/lookupBook';
export const ADD_BOOK = 'addBook/addBook';
export const SET_ADD_BOOK_VALUE = 'addBook/setAddBookValue';
export const CLEAR_ADD_BOOK = 'addBook/clearAddBook';
export const SET_ADD_BOOK_DEFAULT = 'addBook/setAddBookDefault';

export const lookupBook = createThunk(LOOKUP_BOOK);
export const addBook = createThunk(ADD_BOOK);
export const clearAddBook = createAction(CLEAR_ADD_BOOK);
export const setAddBookDefault = createAction(SET_ADD_BOOK_DEFAULT);

export const setAddBookValue = createAction(SET_ADD_BOOK_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({

  [LOOKUP_BOOK]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/book/lookup',
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

  [ADD_BOOK]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const id = payload.id;
    const items = getState().addBook.items;
    const found = _.find(items, { id });
    const newBook = {
      ...structuredClone(found),
      ...payload,
      id: 0
    };

    const promise = createAjaxRequest({
      url: '/book',
      method: 'POST',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify(newBook)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'books', ...data }),

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

  [SET_ADD_BOOK_VALUE]: createSetSettingValueReducer(section),

  [SET_ADD_BOOK_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [CLEAR_ADD_BOOK]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return { ...state, ...otherDefaultState };
  }

}, defaultState, section);
