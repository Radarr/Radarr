import { createAction } from 'redux-actions';
import * as types from './actionTypes';

export const setArtistSort = createAction(types.SET_ARTIST_SORT);
export const setArtistFilter = createAction(types.SET_ARTIST_FILTER);
export const setArtistView = createAction(types.SET_ARTIST_VIEW);
export const setArtistTableOption = createAction(types.SET_ARTIST_TABLE_OPTION);
export const setArtistPosterOption = createAction(types.SET_ARTIST_POSTER_OPTION);
