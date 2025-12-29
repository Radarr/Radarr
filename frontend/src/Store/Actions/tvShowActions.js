import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'tvShows';

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

export const FETCH_TV_SHOWS = 'tvShows/fetchTVShows';
export const SET_TV_SHOW_VALUE = 'tvShows/setTVShowValue';
export const SAVE_TV_SHOW = 'tvShows/saveTVShow';
export const DELETE_TV_SHOW = 'tvShows/deleteTVShow';

export const fetchTVShows = createThunk(FETCH_TV_SHOWS);
export const saveTVShow = createThunk(SAVE_TV_SHOW);
export const deleteTVShow = createThunk(DELETE_TV_SHOW);

export const setTVShowValue = createAction(SET_TV_SHOW_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_TV_SHOWS]: createFetchHandler(section, '/tvshow'),
  [SAVE_TV_SHOW]: createSaveProviderHandler(section, '/tvshow'),
  [DELETE_TV_SHOW]: createRemoveItemHandler(section, '/tvshow')
});

export const reducers = createHandleActions({
  [SET_TV_SHOW_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
