import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import addArtistActionHandlers from './addArtistActionHandlers';

export const lookupArtist = addArtistActionHandlers[types.LOOKUP_ARTIST];
export const addArtist = addArtistActionHandlers[types.ADD_ARTIST];
export const clearAddArtist = createAction(types.CLEAR_ADD_ARTIST);
export const setAddArtistDefault = createAction(types.SET_ADD_ARTIST_DEFAULT);

export const setAddArtistValue = createAction(types.SET_ADD_ARTIST_VALUE, (payload) => {
  return {
    section: 'addArtist',
    ...payload
  };
});
