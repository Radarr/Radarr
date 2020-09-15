import $ from 'jquery';

export default function getJackettIndexers(jackettApi, jackettPath) {
  if (!jackettApi || !jackettPath) {
    return null;
  }

  let indexers = null;
  const ajaxOptions = {
    async: false,
    type: 'GET',
    global: false,
    dataType: 'json',
    url: `${window.Radarr.apiRoot}/jackett?api=${jackettApi}&path=${jackettPath}`,
    success: function(data) {
      indexers = data;
    }
  };

  ajaxOptions.headers = ajaxOptions.headers || {};
  ajaxOptions.headers['X-Api-Key'] = window.Radarr.apiKey;

  $.ajax(ajaxOptions);
  return indexers;
}
