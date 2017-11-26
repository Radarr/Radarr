import $ from 'jquery';
import updateAlbums from 'Utilities/Album/updateAlbums';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleAlbumMonitoredHandler(section) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        albumIds,
        monitored
      } = payload;

      const state = getSectionState(getState(), section, true);

      updateAlbums(dispatch, section, state.items, albumIds, {
        isSaving: true
      });

      const promise = $.ajax({
        url: '/album/monitor',
        method: 'PUT',
        data: JSON.stringify({ albumIds, monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateAlbums(dispatch, section, state.items, albumIds, {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateAlbums(dispatch, section, state.items, albumIds, {
          isSaving: false
        });
      });
    };
  };
}

export default createBatchToggleAlbumMonitoredHandler;
