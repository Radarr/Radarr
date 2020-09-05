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
      localization = data.Strings;
    }
  };

  ajaxOptions.headers = ajaxOptions.headers || {};
  ajaxOptions.headers['X-Api-Key'] = window.Radarr.apiKey;

  $.ajax(ajaxOptions);
  return localization;
}

const translations = getTranslations();

export default function translate(key, args = '') {
  if (args) {
    const translatedKey = translate(key);
    return translatedKey.replace(/\{(\d+)\}/g, (match, index) => {
      return args[index];
    });
  }

  return translations[key] || key;
}
