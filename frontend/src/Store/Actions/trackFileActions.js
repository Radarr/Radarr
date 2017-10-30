import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import trackFileActionHandlers from './trackFileActionHandlers';

export const fetchTrackFiles = trackFileActionHandlers[types.FETCH_TRACK_FILES];
export const deleteTrackFile = trackFileActionHandlers[types.DELETE_TRACK_FILE];
export const deleteTrackFiles = trackFileActionHandlers[types.DELETE_TRACK_FILES];
export const updateTrackFiles = trackFileActionHandlers[types.UPDATE_TRACK_FILES];
export const clearTrackFiles = createAction(types.CLEAR_TRACK_FILES);
