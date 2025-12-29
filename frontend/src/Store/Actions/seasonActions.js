import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'seasons';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'seasonNumber',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {}
};

export const FETCH_SEASONS = 'seasons/fetchSeasons';
export const SET_SEASON_VALUE = 'seasons/setSeasonValue';
export const SAVE_SEASON = 'seasons/saveSeason';
export const DELETE_SEASON = 'seasons/deleteSeason';

export const fetchSeasons = createThunk(FETCH_SEASONS);
export const saveSeason = createThunk(SAVE_SEASON);
export const deleteSeason = createThunk(DELETE_SEASON);

export const setSeasonValue = createAction(SET_SEASON_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_SEASONS]: createFetchHandler(section, '/season'),
  [SAVE_SEASON]: createSaveProviderHandler(section, '/season'),
  [DELETE_SEASON]: createRemoveItemHandler(section, '/season')
});

export const reducers = createHandleActions({
  [SET_SEASON_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
