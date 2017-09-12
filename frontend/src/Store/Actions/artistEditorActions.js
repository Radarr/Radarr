import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import artistEditorActionHandlers from './artistEditorActionHandlers';

export const setArtistEditorSort = createAction(types.SET_ARTIST_EDITOR_SORT);
export const setArtistEditorFilter = createAction(types.SET_ARTIST_EDITOR_FILTER);
export const saveArtistEditor = artistEditorActionHandlers[types.SAVE_ARTIST_EDITOR];
export const bulkDeleteArtist = artistEditorActionHandlers[types.BULK_DELETE_ARTIST];
