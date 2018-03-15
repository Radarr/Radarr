import $ from 'jquery';
import updateAlbums from 'Utilities/Album/updateAlbums';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleAlbumMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      albumIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateAlbums(section, state.items, albumIds, {
      isSaving: true
    }));

    const promise = $.ajax({
      url: '/album/monitor',
      method: 'PUT',
      data: JSON.stringify({ albumIds, monitored }),
      dataType: 'json'
    });

    promise.done(() => {
      dispatch(updateAlbums(section, state.items, albumIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateAlbums(section, state.items, albumIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleAlbumMonitoredHandler;
