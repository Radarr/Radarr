import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import albumEntities from 'Album/albumEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'albums';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  sortKey: 'releaseDate',
  sortDirection: sortDirections.DESCENDING,
  items: [],
  pendingChanges: {},

  columns: [
    {
      name: 'monitored',
      columnLabel: 'Monitored',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'title',
      label: 'Title',
      isVisible: true
    },
    {
      name: 'releaseDate',
      label: 'Release Date',
      isVisible: true
    },
    {
      name: 'secondaryTypes',
      label: 'Secondary Types',
      isVisible: false
    },
    {
      name: 'mediumCount',
      label: 'Media Count',
      isVisible: false
    },
    {
      name: 'trackCount',
      label: 'Track Count',
      isVisible: false
    },
    {
      name: 'duration',
      label: 'Duration',
      isVisible: false
    },
    {
      name: 'status',
      label: 'Status',
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'albums.columns'
];

//
// Actions Types

export const FETCH_ALBUMS = 'albums/fetchAlbums';
export const SET_ALBUMS_SORT = 'albums/setAlbumsSort';
export const SET_ALBUMS_TABLE_OPTION = 'albums/setAlbumsTableOption';
export const CLEAR_ALBUMS = 'albums/clearAlbums';
export const SET_ALBUM_VALUE = 'albums/setAlbumValue';
export const SAVE_ALBUM = 'albums/saveAlbum';
export const TOGGLE_ALBUM_MONITORED = 'albums/toggleAlbumMonitored';
export const TOGGLE_ALBUMS_MONITORED = 'albums/toggleAlbumsMonitored';

//
// Action Creators

export const fetchAlbums = createThunk(FETCH_ALBUMS);
export const setAlbumsSort = createAction(SET_ALBUMS_SORT);
export const setAlbumsTableOption = createAction(SET_ALBUMS_TABLE_OPTION);
export const clearAlbums = createAction(CLEAR_ALBUMS);
export const toggleAlbumMonitored = createThunk(TOGGLE_ALBUM_MONITORED);
export const toggleAlbumsMonitored = createThunk(TOGGLE_ALBUMS_MONITORED);

export const saveAlbum = createThunk(SAVE_ALBUM);

export const setAlbumValue = createAction(SET_ALBUM_VALUE, (payload) => {
  return {
    section: 'albums',
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_ALBUMS]: createFetchHandler(section, '/album'),
  [SAVE_ALBUM]: createSaveProviderHandler(section, '/album'),

  [TOGGLE_ALBUM_MONITORED]: function(getState, payload, dispatch) {
    const {
      albumId,
      albumEntity = albumEntities.ALBUMS,
      monitored
    } = payload;

    const albumSection = _.last(albumEntity.split('.'));

    dispatch(updateItem({
      id: albumId,
      section: albumSection,
      isSaving: true
    }));

    const promise = $.ajax({
      url: `/album/${albumId}`,
      method: 'PUT',
      data: JSON.stringify({ monitored }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(updateItem({
        id: albumId,
        section: albumSection,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id: albumId,
        section: albumSection,
        isSaving: false
      }));
    });
  },

  [TOGGLE_ALBUMS_MONITORED]: function(getState, payload, dispatch) {
    const {
      albumIds,
      albumEntity = albumEntities.ALBUMS,
      monitored
    } = payload;

    const albumSection = _.last(albumEntity.split('.'));

    dispatch(batchActions(
      albumIds.map((albumId) => {
        return updateItem({
          id: albumId,
          section: albumSection,
          isSaving: true
        });
      })
    ));

    const promise = $.ajax({
      url: '/album/monitor',
      method: 'PUT',
      data: JSON.stringify({ albumIds, monitored }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(batchActions(
        albumIds.map((albumId) => {
          return updateItem({
            id: albumId,
            section: albumSection,
            isSaving: false,
            monitored
          });
        })
      ));
    });

    promise.fail((xhr) => {
      dispatch(batchActions(
        albumIds.map((albumId) => {
          return updateItem({
            id: albumId,
            section: albumSection,
            isSaving: false
          });
        })
      ));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_ALBUMS_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_ALBUM_VALUE]: createSetSettingValueReducer(section),

  [CLEAR_ALBUMS]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  },

  [SET_ALBUMS_SORT]: createSetClientSideCollectionSortReducer(section)

}, defaultState, section);
