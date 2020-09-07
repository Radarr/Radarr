import updateBooks from 'Utilities/Book/updateBooks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleBookMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      bookIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateBooks(section, state.items, bookIds, {
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/book/monitor',
      method: 'PUT',
      data: JSON.stringify({ bookIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(updateBooks(section, state.items, bookIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateBooks(section, state.items, bookIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleBookMonitoredHandler;
