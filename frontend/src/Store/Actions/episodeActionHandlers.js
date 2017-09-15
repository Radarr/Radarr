import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import episodeEntities from 'Episode/episodeEntities';
import createFetchHandler from './Creators/createFetchHandler';
import * as types from './actionTypes';
import { updateItem } from './baseActions';

const section = 'episodes';

const episodeActionHandlers = {
  [types.FETCH_EPISODES]: createFetchHandler(section, '/album'),

  [types.TOGGLE_EPISODE_MONITORED]: function(payload) {
    return function(dispatch, getState) {
      const {
        episodeId: id,
        episodeEntity = episodeEntities.EPISODES,
        monitored
      } = payload;

      const episodeSection = _.last(episodeEntity.split('.'));

      dispatch(updateItem({
        id,
        section: episodeSection,
        isSaving: true
      }));

      const promise = $.ajax({
        url: `/album/${id}`,
        method: 'PUT',
        data: JSON.stringify({ monitored }),
        dataType: 'json'
      });

      promise.done((data) => {
        dispatch(updateItem({
          id,
          section: episodeSection,
          isSaving: false,
          monitored
        }));
      });

      promise.fail((xhr) => {
        dispatch(updateItem({
          id,
          section: episodeSection,
          isSaving: false
        }));
      });
    };
  },

  [types.TOGGLE_EPISODES_MONITORED]: function(payload) {
    return function(dispatch, getState) {
      const {
        albumIds,
        episodeEntity = episodeEntities.EPISODES,
        monitored
      } = payload;

      const episodeSection = _.last(episodeEntity.split('.'));

      dispatch(batchActions(
        albumIds.map((episodeId) => {
          return updateItem({
            id: episodeId,
            section: episodeSection,
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
          albumIds.map((episodeId) => {
            return updateItem({
              id: episodeId,
              section: episodeSection,
              isSaving: false,
              monitored
            });
          })
        ));
      });

      promise.fail((xhr) => {
        dispatch(batchActions(
          albumIds.map((episodeId) => {
            return updateItem({
              id: episodeId,
              section: episodeSection,
              isSaving: false
            });
          })
        ));
      });
    };
  }
};

export default episodeActionHandlers;
