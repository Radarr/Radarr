import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import albumStudioActionHandlers from './albumStudioActionHandlers';

export const setAlbumStudioSort = createAction(types.SET_ALBUM_STUDIO_SORT);
export const setAlbumStudioFilter = createAction(types.SET_ALBUM_STUDIO_FILTER);
export const saveAlbumStudio = albumStudioActionHandlers[types.SAVE_ALBUM_STUDIO];
