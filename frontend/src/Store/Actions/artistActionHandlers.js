import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import * as types from './actionTypes';
import createFetchHandler from './Creators/createFetchHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { updateItem } from './baseActions';

const section = 'series';

const artistActionHandlers = {
  [types.FETCH_ARTIST]: createFetchHandler(section, '/artist'),

  [types.SAVE_ARTIST]: createSaveProviderHandler(section,
                                                 '/artist',
                                                 (state) => state.series),

  [types.DELETE_ARTIST]: createRemoveItemHandler(section,
                                                 '/artist',
                                                 (state) => state.series),

  [types.TOGGLE_ARTIST_MONITORED]: function(payload) {
    return function(dispatch, getState) {
      const {
        artistId: id,
        monitored
      } = payload;

      const series = _.find(getState().series.items, { id });

      dispatch(updateItem({
        id,
        section,
        isSaving: true
      }));

      const promise = $.ajax({
        url: `/artist/${id}`,
        method: 'PUT',
        data: JSON.stringify({
          ...series,
          monitored
        }),
        dataType: 'json'
      });

      promise.done((data) => {
        dispatch(updateItem({
          id,
          section,
          isSaving: false,
          monitored
        }));
      });

      promise.fail((xhr) => {
        dispatch(updateItem({
          id,
          section,
          isSaving: false
        }));
      });
    };
  },

  [types.TOGGLE_ALBUM_MONITORED]: function(payload) {
    return function(dispatch, getState) {
      const {
        artistId: id,
        seasonNumber,
        monitored
      } = payload;

      const series = _.find(getState().series.items, { id });
      const seasons = _.cloneDeep(series.seasons);
      const season = _.find(seasons, { seasonNumber });

      season.isSaving = true;

      dispatch(updateItem({
        id,
        section,
        seasons
      }));

      season.monitored = monitored;

      const promise = $.ajax({
        url: `/artist/${id}`,
        method: 'PUT',
        data: JSON.stringify({
          ...series,
          seasons
        }),
        dataType: 'json'
      });

      promise.done((data) => {
        const episodes = _.filter(getState().episodes.items, { artistId: id, seasonNumber });

        dispatch(batchActions([
          updateItem({
            id,
            section,
            ...data
          }),

          ...episodes.map((episode) => {
            return updateItem({
              id: episode.id,
              section: 'episodes',
              monitored
            });
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(updateItem({
          id,
          section,
          seasons: series.seasons
        }));
      });
    };
  }
};

export default artistActionHandlers;
