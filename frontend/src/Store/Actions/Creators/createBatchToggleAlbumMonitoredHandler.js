import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateAlbums from 'Utilities/Album/updateAlbums';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleAlbumMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      bookIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateAlbums(section, state.items, bookIds, {
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/album/monitor',
      method: 'PUT',
      data: JSON.stringify({ bookIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(updateAlbums(section, state.items, bookIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateAlbums(section, state.items, bookIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleAlbumMonitoredHandler;
