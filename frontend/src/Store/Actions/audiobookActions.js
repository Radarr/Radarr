import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'audiobooks';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {}
};

export const FETCH_AUDIOBOOKS = 'audiobooks/fetchAudiobooks';
export const SET_AUDIOBOOK_VALUE = 'audiobooks/setAudiobookValue';
export const SAVE_AUDIOBOOK = 'audiobooks/saveAudiobook';
export const DELETE_AUDIOBOOK = 'audiobooks/deleteAudiobook';

export const fetchAudiobooks = createThunk(FETCH_AUDIOBOOKS);
export const saveAudiobook = createThunk(SAVE_AUDIOBOOK);
export const deleteAudiobook = createThunk(DELETE_AUDIOBOOK, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles
    }
  };
});

export const setAudiobookValue = createAction(SET_AUDIOBOOK_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_AUDIOBOOKS]: createFetchHandler(section, '/audiobook'),
  [SAVE_AUDIOBOOK]: createSaveProviderHandler(section, '/audiobook'),
  [DELETE_AUDIOBOOK]: createRemoveItemHandler(section, '/audiobook')
});

export const reducers = createHandleActions({
  [SET_AUDIOBOOK_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
