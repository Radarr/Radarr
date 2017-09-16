import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import artistActionHandlers from './artistActionHandlers';

export const fetchArtist = artistActionHandlers[types.FETCH_ARTIST];
export const saveArtist = artistActionHandlers[types.SAVE_ARTIST];
export const deleteArtist = artistActionHandlers[types.DELETE_ARTIST];
export const toggleSeriesMonitored = artistActionHandlers[types.TOGGLE_ARTIST_MONITORED];
export const toggleSeasonMonitored = artistActionHandlers[types.TOGGLE_ALBUM_MONITORED];

export const setArtistValue = createAction(types.SET_ARTIST_VALUE, (payload) => {
  return {
    section: 'series',
    ...payload
  };
});
