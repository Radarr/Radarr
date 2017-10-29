import $ from 'jquery';
import updateAlbums from 'Utilities/Album/updateAlbums';

function createToggleAlbumMonitoredHandler(section, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        albumId,
        monitored
      } = payload;

      const state = getFromState(getState());

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
