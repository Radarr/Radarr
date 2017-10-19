import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import episodeEntities from 'Album/episodeEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import * as types from './actionTypes';
import { set, removeItem, updateItem } from './baseActions';

const section = 'trackFiles';
const deleteTrackFile = createRemoveItemHandler(section, '/trackFile');

const trackFileActionHandlers = {
  [types.FETCH_TRACK_FILES]: createFetchHandler(section, '/trackFile'),

  [types.DELETE_TRACK_FILE]: function(payload) {
    return function(dispatch, getState) {
      const {
        id: trackFileId,
        episodeEntity = episodeEntities.EPISODES
      } = payload;

      const episodeSection = _.last(episodeEntity.split('.'));

      const deletePromise = deleteTrackFile(payload)(dispatch, getState);

      deletePromise.done(() => {
        const episodes = getState().episodes.items;
        const episodesWithRemovedFiles = _.filter(episodes, { trackFileId });

        dispatch(batchActions([
          ...episodesWithRemovedFiles.map((episode) => {
            return updateItem({
              section: episodeSection,
              ...episode,
              trackFileId: 0,
              hasFile: false
            });
          })
        ]));
      });
    };
  },

  [types.DELETE_TRACK_FILES]: function(payload) {
    return function(dispatch, getState) {
      const {
        trackFileIds
      } = payload;

      dispatch(set({ section, isDeleting: true }));

      const promise = $.ajax({
        url: '/trackFile/bulk',
        method: 'DELETE',
        dataType: 'json',
        data: JSON.stringify({ trackFileIds })
      });

      promise.done(() => {
        const episodes = getState().episodes.items;
        const episodesWithRemovedFiles = trackFileIds.reduce((acc, trackFileId) => {
          acc.push(..._.filter(episodes, { trackFileId }));

          return acc;
        }, []);

        dispatch(batchActions([
          ...trackFileIds.map((id) => {
            return removeItem({ section, id });
          }),

          ...episodesWithRemovedFiles.map((episode) => {
            return updateItem({
              section: 'episodes',
              ...episode,
              trackFileId: 0,
              hasFile: false
            });
          }),

          set({
            section,
            isDeleting: false,
            deleteError: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isDeleting: false,
          deleteError: xhr
        }));
      });
    };
  },

  [types.UPDATE_TRACK_FILES]: function(payload) {
    return function(dispatch, getState) {
      const {
        trackFileIds,
        language,
        quality
      } = payload;

      dispatch(set({ section, isSaving: true }));

      const data = {
        trackFileIds
      };

      if (language) {
        data.language = language;
      }

      if (quality) {
        data.quality = quality;
      }

      const promise = $.ajax({
        url: '/trackFile/editor',
        method: 'PUT',
        dataType: 'json',
        data: JSON.stringify(data)
      });

      promise.done(() => {
        dispatch(batchActions([
          ...trackFileIds.map((id) => {
            const props = {};

            if (language) {
              props.language = language;
            }

            if (quality) {
              props.quality = quality;
            }

            return updateItem({ section, id, ...props });
          }),

          set({
            section,
            isSaving: false,
            saveError: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isSaving: false,
          saveError: xhr
        }));
      });
    };
  }
};

export default trackFileActionHandlers;
