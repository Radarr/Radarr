import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import importArtistActionHandlers from './importArtistActionHandlers';

export const queueLookupSeries = importArtistActionHandlers[types.QUEUE_LOOKUP_ARTIST];
export const startLookupSeries = importArtistActionHandlers[types.START_LOOKUP_ARTIST];
export const importArtist = importArtistActionHandlers[types.IMPORT_ARTIST];
export const clearImportArtist = createAction(types.CLEAR_IMPORT_ARTIST);

export const setImportArtistValue = createAction(types.SET_IMPORT_ARTIST_VALUE, (payload) => {
  return {

    section: 'importArtist',
    ...payload
  };
});
