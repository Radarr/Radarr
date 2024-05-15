import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from '../baseActions';

function createPurgeHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isPurging: true }));

    const ajaxOptions = {
      url: `${url}/purge`,
      method: 'POST'
    };

    const { request } = createAjaxRequest(ajaxOptions);

    request.done((data) => {
      dispatch(
        set({
          section,
          isPurging: false,
          purgeError: null
        })
      );
    });

    request.fail((xhr) => {
      dispatch(
        set({
          section,
          isPurging: false
        })
      );
    });
  };
}

export default createPurgeHandler;
