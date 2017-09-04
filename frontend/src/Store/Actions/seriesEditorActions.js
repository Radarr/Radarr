import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import seriesEditorActionHandlers from './seriesEditorActionHandlers';

export const setSeriesEditorSort = createAction(types.SET_SERIES_EDITOR_SORT);
export const setSeriesEditorFilter = createAction(types.SET_SERIES_EDITOR_FILTER);
export const saveArtistEditor = seriesEditorActionHandlers[types.SAVE_ARTIST_EDITOR];
export const bulkDeleteArtist = seriesEditorActionHandlers[types.BULK_DELETE_ARTIST];
