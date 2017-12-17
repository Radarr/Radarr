import $ from 'jquery';
import updateAlbums from 'Utilities/Album/updateAlbums';
import getSectionState from 'Utilities/State/getSectionState';

function createToggleAlbumMonitoredHandler(section) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        albumId,
        monitored
      } = payload;

      const state = getSectionState(getState(), section, true);

      updateAlbums(dispatch, section, state.items, [albumId], {
        isSaving: true
      });

      const promise = $.ajax({
        url: `/album/${albumId}`,
        method: 'PUT',
        data: JSON.stringify({ monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateAlbums(dispatch, section, state.items, [albumId], {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateAlbums(dispatch, section, state.items, [albumId], {
          isSaving: false
        });
      });
    };
  };
}

export default createToggleAlbumMonitoredHandler;
