import $ from 'jquery';
import updateEpisodes from 'Utilities/Episode/updateEpisodes';

function createBatchToggleEpisodeMonitoredHandler(section, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        albumIds,
        monitored
      } = payload;

      const state = getFromState(getState());

      updateEpisodes(dispatch, section, state.items, albumIds, {
        isSaving: true
      });

      const promise = $.ajax({
        url: '/episode/monitor',
        method: 'PUT',
        data: JSON.stringify({ albumIds, monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateEpisodes(dispatch, section, state.items, albumIds, {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateEpisodes(dispatch, section, state.items, albumIds, {
          isSaving: false
        });
      });
    };
  };
}

export default createBatchToggleEpisodeMonitoredHandler;
