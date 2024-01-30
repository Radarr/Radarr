import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateMovies from 'Utilities/Movie/updateMovies';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleMovieMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      movieIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateMovies(section, state.items, movieIds, {
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/movie/editor',
      method: 'PUT',
      data: JSON.stringify({ movieIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(updateMovies(section, state.items, movieIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateMovies(section, state.items, movieIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleMovieMonitoredHandler;
