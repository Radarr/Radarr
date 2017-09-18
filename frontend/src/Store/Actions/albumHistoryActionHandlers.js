import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import * as types from './actionTypes';
import { set, update } from './baseActions';
import { fetchAlbumHistory } from './albumHistoryActions';

const albumHistoryActionHandlers = {
  [types.FETCH_ALBUM_HISTORY]: function(payload) {
    const section = 'albumHistory';

    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      const queryParams = {
        pageSize: 1000,
        page: 1,
        filterKey: 'albumId',
        filterValue: payload.albumId,
        sortKey: 'date',
        sortDirection: sortDirections.DESCENDING
      };

      const promise = $.ajax({
        url: '/history',
        data: queryParams
      });

      promise.done((data) => {
        dispatch(batchActions([
          update({ section, data: data.records }),

          set({
            section,
            isFetching: false,
            isPopulated: true,
            error: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isFetching: false,
          isPopulated: false,
          error: xhr
        }));
      });
    };
  },

  [types.ALBUM_HISTORY_MARK_AS_FAILED]: function(payload) {
    return function(dispatch, getState) {
      const {
        historyId,
        albumId
      } = payload;

      const promise = $.ajax({
        url: '/history/failed',
        method: 'POST',
        data: {
          id: historyId
        }
      });

      promise.done(() => {
        dispatch(fetchAlbumHistory({ albumId }));
      });
    };
  }
};

export default albumHistoryActionHandlers;
