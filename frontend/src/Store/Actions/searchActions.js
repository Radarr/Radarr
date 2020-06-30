import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import monitorOptions from 'Utilities/Author/monitorOptions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewAuthor from 'Utilities/Author/getNewAuthor';
import getNewBook from 'Utilities/Book/getNewBook';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set, update, updateItem } from './baseActions';

//
// Variables

export const section = 'search';
let abortCurrentRequest = null;

//
// State

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
    monitor: monitorOptions[0].key,
    qualityProfileId: 0,
    metadataProfileId: 0,
    tags: []
  }
};

export const persistState = [
  'search.defaults'
];

//
// Actions Types

export const GET_SEARCH_RESULTS = 'search/getSearchResults';
export const ADD_AUTHOR = 'search/addAuthor';
export const ADD_BOOK = 'search/addBook';
export const CLEAR_SEARCH_RESULTS = 'search/clearSearchResults';
export const SET_ADD_DEFAULT = 'search/setAddDefault';

//
// Action Creators

export const getSearchResults = createThunk(GET_SEARCH_RESULTS);
export const addAuthor = createThunk(ADD_AUTHOR);
export const addBook = createThunk(ADD_BOOK);
export const clearSearchResults = createAction(CLEAR_SEARCH_RESULTS);
export const setAddDefault = createAction(SET_ADD_DEFAULT);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [GET_SEARCH_RESULTS]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/search',
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

  [ADD_AUTHOR]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const foreignAuthorId = payload.foreignAuthorId;
    const items = getState().search.items;
    const itemToAdd = _.find(items, { foreignId: foreignAuthorId });
    const newAuthor = getNewAuthor(_.cloneDeep(itemToAdd.author), payload);

    const promise = createAjaxRequest({
      url: '/author',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(newAuthor)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'authors', ...data }),

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
  },

  [ADD_BOOK]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const foreignBookId = payload.foreignBookId;
    const items = getState().search.items;
    const itemToAdd = _.find(items, { foreignId: foreignBookId });
    const newBook = getNewBook(_.cloneDeep(itemToAdd.book), payload);

    const promise = createAjaxRequest({
      url: '/book',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(newBook)
    }).request;

    promise.done((data) => {
      data.editions = itemToAdd.book.editions;
      itemToAdd.book = data;
      dispatch(batchActions([
        updateItem({ section: 'authors', ...data.author }),
        updateItem({ section, ...itemToAdd }),

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

//
// Reducers

export const reducers = createHandleActions({

  [SET_ADD_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [CLEAR_SEARCH_RESULTS]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState, section);
