import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import albumHistoryActionHandlers from './albumHistoryActionHandlers';

export const fetchAlbumHistory = albumHistoryActionHandlers[types.FETCH_ALBUM_HISTORY];
export const clearAlbumHistory = createAction(types.CLEAR_ALBUM_HISTORY);
export const albumHistoryMarkAsFailed = albumHistoryActionHandlers[types.ALBUM_HISTORY_MARK_AS_FAILED];
