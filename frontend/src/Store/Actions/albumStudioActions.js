import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import albumStudioActionHandlers from './albumStudioActionHandlers';

export const setAlbumStudioSort = createAction(types.SET_SEASON_PASS_SORT);
export const setAlbumStudioFilter = createAction(types.SET_SEASON_PASS_FILTER);
export const saveAlbumStudio = albumStudioActionHandlers[types.SAVE_SEASON_PASS];
