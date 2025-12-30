import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

export const section = 'episodes';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'episodeNumber',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {}
};

export const FETCH_EPISODES = 'episodes/fetchEpisodes';
export const SET_EPISODE_VALUE = 'episodes/setEpisodeValue';
export const SAVE_EPISODE = 'episodes/saveEpisode';
export const DELETE_EPISODE = 'episodes/deleteEpisode';

export const fetchEpisodes = createThunk(FETCH_EPISODES);
export const saveEpisode = createThunk(SAVE_EPISODE);
export const deleteEpisode = createThunk(DELETE_EPISODE);

export const setEpisodeValue = createAction(SET_EPISODE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const actionHandlers = handleThunks({
  [FETCH_EPISODES]: createFetchHandler(section, '/episode'),
  [SAVE_EPISODE]: createSaveProviderHandler(section, '/episode'),
  [DELETE_EPISODE]: createRemoveItemHandler(section, '/episode')
});

export const reducers = createHandleActions({
  [SET_EPISODE_VALUE]: createSetSettingValueReducer(section)
}, defaultState, section);
