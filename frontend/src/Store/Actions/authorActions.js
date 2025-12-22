import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'authors';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'sortName',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {}
};

export const FETCH_AUTHORS = 'authors/fetchAuthors';
export const SET_AUTHOR_VALUE = 'authors/setAuthorValue';
export const SAVE_AUTHOR = 'authors/saveAuthor';
export const DELETE_AUTHOR = 'authors/deleteAuthor';

export const fetchAuthors = createThunk(FETCH_AUTHORS);
export const saveAuthor = createThunk(SAVE_AUTHOR);
export const deleteAuthor = createThunk(DELETE_AUTHOR, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles
    }
  };
});

export const setAuthorValue = createAction(SET_AUTHOR_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_AUTHORS]: createFetchHandler(section, '/author'),
  [SAVE_AUTHOR]: createSaveProviderHandler(section, '/author'),
  [DELETE_AUTHOR]: createRemoveItemHandler(section, '/author')
});

export const reducers = createHandleActions({
  [SET_AUTHOR_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
