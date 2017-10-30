import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import trackActionHandlers from './trackActionHandlers';

export const fetchTracks = trackActionHandlers[types.FETCH_TRACKS];
export const setTracksSort = createAction(types.SET_TRACKS_SORT);
export const setTracksTableOption = createAction(types.SET_TRACKS_TABLE_OPTION);
export const clearTracks = createAction(types.CLEAR_TRACKS);
