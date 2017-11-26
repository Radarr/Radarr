import _ from 'lodash';
import $ from 'jquery';
import getMonitoringOptions from 'Utilities/Artist/getMonitoringOptions';
import * as types from './actionTypes';
import { set } from './baseActions';
import { fetchArtist } from './artistActions';

const section = 'albumStudio';

const albumStudioActionHandlers = {
  [types.SAVE_ALBUM_STUDIO]: function(payload) {
    return function(dispatch, getState) {
      const {
        artistIds,
        monitored,
        monitor
      } = payload;

      let monitoringOptions = null;
      const artist = [];
      const allArtists = getState().artist.items;

      artistIds.forEach((id) => {
        const s = _.find(allArtists, { id });
        const artistToUpdate = { id };

        if (payload.hasOwnProperty('monitored')) {
          artistToUpdate.monitored = monitored;
        }

        if (monitor) {
          const {
            albums,
            options: artistMonitoringOptions
          } = getMonitoringOptions(_.cloneDeep(s.albums), monitor);

          if (!monitoringOptions) {
            monitoringOptions = artistMonitoringOptions;
          }

          artistToUpdate.albums = albums;
        }

        artist.push(artistToUpdate);
      });

      dispatch(set({
        section,
        isSaving: true
      }));

      const promise = $.ajax({
        url: '/albumStudio',
        method: 'POST',
        data: JSON.stringify({
          artist,
          monitoringOptions
        }),
        dataType: 'json'
      });

      promise.done((data) => {
        dispatch(fetchArtist());

        dispatch(set({
          section,
          isSaving: false,
          saveError: null
        }));
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

export default albumStudioActionHandlers;
