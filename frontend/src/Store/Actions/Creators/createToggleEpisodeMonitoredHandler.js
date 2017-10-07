import $ from 'jquery';
import updateEpisodes from 'Utilities/Episode/updateEpisodes';

function createToggleEpisodeMonitoredHandler(section, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        albumId,
        monitored
      } = payload;

      const state = getFromState(getState());

      updateEpisodes(dispatch, section, state.items, [albumId], {
        isSaving: true
      });

      const promise = $.ajax({
        url: `/episode/${albumId}`,
        method: 'PUT',
        data: JSON.stringify({ monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateEpisodes(dispatch, section, state.items, [albumId], {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateEpisodes(dispatch, section, state.items, [albumId], {
          isSaving: false
        });
      });
    };
  };
}

export default createToggleEpisodeMonitoredHandler;
