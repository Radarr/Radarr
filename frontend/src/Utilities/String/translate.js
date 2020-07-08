import $ from 'jquery';

function getTranslations() {
  let localization = null;
  const ajaxOptions = {
    async: false,
    type: 'GET',
    global: false,
    dataType: 'json',
    url: `${window.Radarr.apiRoot}/localization`,
    success: function(data) {
      localization = data.strings;
    }
  };

  ajaxOptions.headers = ajaxOptions.headers || {};
  ajaxOptions.headers['X-Api-Key'] = window.Radarr.apiKey;

  $.ajax(ajaxOptions);
  return localization;
}

const translations = getTranslations();

export default function translate(key) {
  const formatedKey = key.charAt(0).toLowerCase() + key.slice(1);
  return translations[formatedKey] || key;
}
