import _ from 'lodash';
import $ from 'jquery';
import getMonitoringOptions from 'Utilities/Series/getMonitoringOptions';
import * as types from './actionTypes';
import { set } from './baseActions';
import { fetchArtist } from './artistActions';

const section = 'albumStudio';

const albumStudioActionHandlers = {
  [types.SAVE_SEASON_PASS]: function(payload) {
    return function(dispatch, getState) {
      const {
        artistIds,
        monitored,
        monitor
      } = payload;

      let monitoringOptions = null;
      const series = [];
      const allSeries = getState().series.items;

      artistIds.forEach((id) => {
        const s = _.find(allSeries, { id });
        const seriesToUpdate = { id };

        if (payload.hasOwnProperty('monitored')) {
          seriesToUpdate.monitored = monitored;
        }

        if (monitor) {
          const {
            seasons,
            options: seriesMonitoringOptions
          } = getMonitoringOptions(_.cloneDeep(s.seasons), monitor);

          if (!monitoringOptions) {
            monitoringOptions = seriesMonitoringOptions;
          }

          seriesToUpdate.seasons = seasons;
        }

        series.push(seriesToUpdate);
      });

      dispatch(set({
        section,
        isSaving: true
      }));

      const promise = $.ajax({
        url: '/albumStudio',
        method: 'POST',
        data: JSON.stringify({
          series,
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
